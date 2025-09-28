# Projeto de Grafos / Graph Theory Project

Este projeto implementa algoritmos clássicos de **Teoria dos Grafos** e lê grafos a partir de arquivos JSON na pasta `graphs/`.
This project implements classic **Graph Theory** algorithms and reads graphs from JSON files located in the `graphs/` folder.
> 💻 Um executável para **macOS** já foi gerado e está disponível para download na seção **Releases** do repositório GitHub.

> 💻 A **macOS executable** has already been built and is available for download in the **Releases** section of the GitHub repository.


---

## 🇧🇷 Versão em Português

### Algoritmos Implementados
- **BFS/DFS** (Busca em Largura / Profundidade)
- **Dijkstra**, **A\***, **Best-First**
- **Ciclos/Caminhos Eulerianos e Hamiltonianos**
- **Isomorfismo de Grafos**
- **Árvore Geradora Mínima** (Kruskal)

### Estrutura dos Arquivos JSON
Cada grafo é definido em um arquivo dentro de `graphs/` usando o seguinte formato:

```jsonc
{
  "directed": false,           // true = dirigido, false = não-dirigido
  "vertices": [
    {
      "name": "A",            // nome do vértice
      "x": 0, "y": 0,         // (opcional) coordenadas para heurísticas (ex.: A*)
      "adj": [
        { "To": "B", "weight": 1 },  // aresta A -> B com peso 1
        { "To": "E", "weight": 1 }
      ]
    }
  ]
}
```

**Observações**
- Os campos `x` e `y` aparecem apenas quando necessários (ex.: `dijkstra_astar_bestfirst.json`).
- `directed = true` indica grafo **dirigido**; `false`, **não-dirigido**.
- A lista `adj` define as adjacências de cada vértice e o **peso** de cada aresta.

### Como testar com outros grafos
1. Crie um novo arquivo `.json` seguindo o formato acima **ou** edite um existente.
2. Salve o arquivo dentro da pasta **`graphs/`** (é a pasta que o app lê).
3. Execute o programa normalmente; ele utilizará os JSONs presentes em `graphs/`.

### Arquivos de exemplo incluídos
- `bfs_dfs.json` — exemplos para BFS/DFS.
- `dijkstra_astar_bestfirst.json` — exemplos para Dijkstra / A* / Best-First (com coordenadas `x`,`y`).
- `euler_directed.json` — grafo para Euler **dirigido**.
- `euler_undirected.json` — grafo para Euler **não-dirigido**.
- `hamiltonian.json` — grafo para caminhos/ciclos Hamiltonianos.
- `iso_g1.json` e `iso_g2.json` — par de grafos para **isomorfismo**.
- `mst_kruskal.json` — grafo para **Árvore Geradora Mínima** (Kruskal).

---

## 🇬🇧 English Version

### Implemented Algorithms
- **BFS/DFS** (Breadth-First / Depth-First Search)
- **Dijkstra**, **A\***, **Best-First**
- **Eulerian and Hamiltonian** Paths/Circuits
- **Graph Isomorphism**
- **Minimum Spanning Tree** (Kruskal)

### JSON File Structure
Each graph is defined in a file under `graphs/` using the following format:

```jsonc
{
  "directed": false,           // true = directed, false = undirected
  "vertices": [
    {
      "name": "A",            // vertex name
      "x": 0, "y": 0,         // (optional) coordinates for heuristics (e.g., A*)
      "adj": [
        { "To": "B", "weight": 1 },  // edge A -> B with weight 1
        { "To": "E", "weight": 1 }
      ]
    }
  ]
}
```

**Notes**
- `x` and `y` appear only when needed (e.g., `dijkstra_astar_bestfirst.json`).
- `directed = true` means a **directed** graph; `false` means **undirected**.
- The `adj` list defines each vertex adjacency and the **edge weight**.

### Adding/Testing Other Graphs
1. Create a new `.json` file following the format above **or** edit an existing one.
2. Save it into the **`graphs/`** folder (this is the folder the app reads from).
3. Run the program as usual; it will use the JSON files inside `graphs/`.

### Included Sample Files
- `bfs_dfs.json` — samples for BFS/DFS.
- `dijkstra_astar_bestfirst.json` — samples for Dijkstra / A* / Best-First (with `x`,`y` coordinates).
- `euler_directed.json` — **directed** Eulerian graph.
- `euler_undirected.json` — **undirected** Eulerian graph.
- `hamiltonian.json` — graph for Hamiltonian paths/circuits.
- `iso_g1.json` & `iso_g2.json` — graph pair for **isomorphism**.
- `mst_kruskal.json` — graph for **Minimum Spanning Tree** (Kruskal).
