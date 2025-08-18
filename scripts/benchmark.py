import requests
import faker
import time
import argparse
import numpy as np
import random
import threading
from collections import defaultdict
from concurrent.futures import ThreadPoolExecutor, as_completed


BASE_URL = 'http://localhost:5099/shorten'


ACTIONS = ['POST', 'GET', 'GET_STATS', 'PUT', 'DELETE']
WEIGHTS = [0.05,   0.85,  0.05,        0.025, 0.025] 

created_urls_lock = threading.Lock()
created_urls = []



def api_call(session: requests.Session, method: str, url: str, **kwargs):
    """Função genérica para realizar uma chamada de API e medir o tempo."""
    start_time = time.time()
    try:
        response = session.request(method, url, timeout=30, **kwargs)
        duration = time.time() - start_time
        return duration, response.status_code, response
    except requests.exceptions.RequestException:
        duration = time.time() - start_time
        return duration, 0, None

def do_post(session: requests.Session):
    """Cria uma nova URL encurtada."""
    fake = faker.Faker()
    data = {'url': fake.url()}
    duration, status, response = api_call(session, 'POST', BASE_URL, json=data)
    
    new_code = None
    if status == 201:
        try:
            new_code = response.json().get('shortCode')
            if new_code:
                with created_urls_lock:
                    created_urls.append(new_code)
        except (requests.exceptions.JSONDecodeError, AttributeError):
            pass 
            
    return duration, status

def do_get(session: requests.Session, short_code: str):
    """Acessa/redireciona uma URL encurtada."""
    url = f"{BASE_URL}/{short_code}"
   
    duration, status, _ = api_call(session, 'GET', url, allow_redirects=False)
    return duration, status

def do_get_stats(session: requests.Session, short_code: str):
    """Busca as estatísticas de uma URL encurtada."""
    url = f"{BASE_URL}/{short_code}/stats"
    duration, status, _ = api_call(session, 'GET', url)
    return duration, status

def do_put(session: requests.Session, short_code: str):
    """Atualiza uma URL encurtada."""
    fake = faker.Faker()
    url = f"{BASE_URL}/{short_code}"
    data = {'url': fake.url()}
    duration, status, _ = api_call(session, 'PUT', url, json=data)
    return duration, status

def do_delete(session: requests.Session, short_code: str):
    """Deleta uma URL encurtada."""
    url = f"{BASE_URL}/{short_code}"
    duration, status, _ = api_call(session, 'DELETE', url)
    if status == 204: # No Content (sucesso)
        with created_urls_lock:
            if short_code in created_urls:
                created_urls.remove(short_code)
    return duration, status

# --- Lógica do Worker ---

def run_task(session: requests.Session):
    """
    Executa uma única tarefa, simulando o comportamento de um usuário.
    """
    action = random.choices(ACTIONS, WEIGHTS)[0]
    
    short_code_to_use = None
    if action != 'POST':
        with created_urls_lock:
            if created_urls:
                short_code_to_use = random.choice(created_urls)
        
       
        if not short_code_to_use:
            action = 'POST'

   
    if action == 'POST':
        duration, status = do_post(session)
    elif action == 'GET':
        duration, status = do_get(session, short_code_to_use)
    elif action == 'GET_STATS':
        duration, status = do_get_stats(session, short_code_to_use)
    elif action == 'PUT':
        duration, status = do_put(session, short_code_to_use)
    elif action == 'DELETE':
        duration, status = do_delete(session, short_code_to_use)
    else:
        return 0, 0, "UNKNOWN"

    return duration, status, action


def benchmark(jobs: int, total_tasks: int, max_time: int):
    print(f"Iniciando benchmark com {jobs} usuários concorrentes, até {total_tasks} tarefas ou {max_time}s.")
    
    results = []
    start_time = time.time()

    with ThreadPoolExecutor(max_workers=jobs) as executor:
        sessions = [requests.Session() for _ in range(jobs)]
        futures = {executor.submit(run_task, sessions[i % jobs]) for i in range(total_tasks)}
        
        for i, future in enumerate(as_completed(futures)):
            if max_time > 0 and (time.time() - start_time) > max_time:
                print("\nLimite de tempo atingido. Encerrando...")
                for f in futures:
                    if not f.done(): f.cancel()
                break
            
            try:
                duration, status, action = future.result()
                results.append({'duration': duration, 'status': status, 'action': action})
                print(f"\rTarefas concluídas: {i + 1}/{total_tasks}", end="")
            except Exception:
                pass

    print("\nBenchmark finalizado.")
    
   
    end_time = time.time()
    elapsed_time = end_time - start_time
    
    if not results:
        print("Nenhuma requisição foi concluída.")
        return


    report_data = defaultdict(list)
    for r in results:
        report_data[r['action']].append(r)

  
    print("\n--- Resultados Gerais ---")
    print(f"Tempo total decorrido: {elapsed_time:.2f} segundos")
    print(f"Total de requisições concluídas: {len(results)}")
    print(f"Requisições por segundo (RPS): {len(results) / elapsed_time:.2f}")


    print("\n--- Detalhes por Endpoint ---")
    for action, action_results in sorted(report_data.items()):
        durations = [r['duration'] for r in action_results]
        success_count = sum(1 for r in action_results if 200 <= r['status'] < 300 or r['status'] == 302) 
        failed_count = len(action_results) - success_count
        
        print(f"\n-> Endpoint: {action}")
        print(f"   - Total de Requisições: {len(action_results)} ({len(action_results)/len(results):.1%})")
        print(f"   - Sucesso / Falha: {success_count} / {failed_count}")
        print(f"   - Latência (ms):")
        print(f"     - Média:   {np.mean(durations)*1000:.2f}")
        print(f"     - p50:     {np.percentile(durations, 50)*1000:.2f}")
        print(f"     - p95:     {np.percentile(durations, 95)*1000:.2f}")
        print(f"     - p99:     {np.percentile(durations, 99)*1000:.2f}")

def main():
    parser = argparse.ArgumentParser(description="Ferramenta de benchmark realista para a API de encurtamento de URL.")
    parser.add_argument("-j", "--jobs", type=int, default=10, help="Número de usuários concorrentes (threads). Padrão: 10")
    parser.add_argument("-n", "--tasks", type=int, default=1000, help="Número total de requisições a serem feitas. Padrão: 1000")
    parser.add_argument("-t", "--time", type=int, default=0, help="Tempo máximo de execução em segundos (0 para sem limite). Padrão: 0")
    args = parser.parse_args()

    benchmark(args.jobs, args.tasks, args.time)

if __name__ == "__main__":
    main()
