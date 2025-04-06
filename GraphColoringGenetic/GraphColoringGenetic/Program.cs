
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GraphColoring
{
    /// <summary>
    /// Reprezentacja grafu
    /// </summary>
    class Graph
    {
        public int VertexCount { get; private set; }
        public bool[,] AdjacencyMatrix { get; private set; }

        public Graph(int vertexCount)
        {
            VertexCount = vertexCount;
            AdjacencyMatrix = new bool[vertexCount, vertexCount];
        }

        public void AddEdge(int v1, int v2)
        {
            AdjacencyMatrix[v1, v2] = true;
            AdjacencyMatrix[v2, v1] = true;
        }

        public bool AreAdjacent(int v1, int v2)
        {
            return AdjacencyMatrix[v1, v2];
        }

        public override string ToString()
        {
            string result = $"Graf o {VertexCount} wierzchołkach\n";
            for (int i = 0; i < VertexCount; i++)
            {
                result += $"Wierzchołek {i} połączony z: ";
                for (int j = 0; j < VertexCount; j++)
                {
                    if (AdjacencyMatrix[i, j])
                        result += $"{j} ";
                }
                result += "\n";
            }
            return result;
        }
    }

    /// <summary>
    /// Generator instancji problemów kolorowania grafu
    /// </summary>
    class GraphGenerator
    {
        private Random random;

        public GraphGenerator(int seed = 0)
        {
            random = seed == 0 ? new Random() : new Random(seed);
        }

        // Generuje graf losowy o zadanej liczbie wierzchołków i gęstości
        public Graph GenerateRandomGraph(int vertexCount, double density)
        {
            Graph graph = new Graph(vertexCount);
            
            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = i + 1; j < vertexCount; j++)
                {
                    if (random.NextDouble() < density)
                    {
                        graph.AddEdge(i, j);
                    }
                }
            }
            
            return graph;
        }

        // Generuje graf k-regularny (każdy wierzchołek ma dokładnie k sąsiadów)
        public Graph GenerateRegularGraph(int vertexCount, int k)
        {
            if (k >= vertexCount || (k * vertexCount) % 2 != 0)
                throw new ArgumentException("Nie można utworzyć grafu k-regularnego o podanych parametrach");

            Graph graph = new Graph(vertexCount);
            List<int>[] tempEdges = new List<int>[vertexCount];
            for (int i = 0; i < vertexCount; i++)
                tempEdges[i] = new List<int>();

            // Algorytm losowego generowania grafu k-regularnego
            List<int> availableVertices = new List<int>();
            
            while (true)
            {
                bool allSatisfied = true;
                availableVertices.Clear();
                
                for (int i = 0; i < vertexCount; i++)
                {
                    if (tempEdges[i].Count < k)
                    {
                        availableVertices.Add(i);
                        allSatisfied = false;
                    }
                }
                
                if (allSatisfied)
                    break;
                
                if (availableVertices.Count < 2)
                {
                    // Restart, jeśli nie można spełnić warunku k-regularności
                    for (int i = 0; i < vertexCount; i++)
                        tempEdges[i].Clear();
                    continue;
                }
                
                // Wybierz dwa losowe wierzchołki, które nie są jeszcze połączone
                int attempts = 0;
                int v1, v2;
                do
                {
                    int idx1 = random.Next(availableVertices.Count);
                    int idx2 = random.Next(availableVertices.Count - 1);
                    if (idx2 >= idx1) idx2++;
                    
                    v1 = availableVertices[idx1];
                    v2 = availableVertices[idx2];
                    
                    attempts++;
                    if (attempts > 100)
                    {
                        // Restart, jeśli trudno znaleźć pasujące wierzchołki
                        for (int i = 0; i < vertexCount; i++)
                            tempEdges[i].Clear();
                        break;
                    }
                } while (tempEdges[v1].Contains(v2));
                
                if (attempts > 100)
                    continue;
                
                tempEdges[v1].Add(v2);
                tempEdges[v2].Add(v1);
            }
            
            // Konwersja listy krawędzi na macierz sąsiedztwa
            for (int i = 0; i < vertexCount; i++)
            {
                foreach (int j in tempEdges[i])
                {
                    graph.AddEdge(i, j);
                }
            }
            
            return graph;
        }

        // Generuje graf planarny
        public Graph GeneratePlanarGraph(int vertexCount, double edgeProbability = 0.5)
        {
            Graph graph = new Graph(vertexCount);
            
            // Generowanie grafu planarnego jako podgrafu siatki
            int gridSize = (int)Math.Ceiling(Math.Sqrt(vertexCount));
            int[] xCoord = new int[vertexCount];
            int[] yCoord = new int[vertexCount];
            
            // Przypisanie współrzędnych
            for (int i = 0; i < vertexCount; i++)
            {
                xCoord[i] = i % gridSize;
                yCoord[i] = i / gridSize;
            }
            
            // Dodawanie krawędzi tylko między sąsiednimi punktami siatki
            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = i + 1; j < vertexCount; j++)
                {
                    int dx = Math.Abs(xCoord[i] - xCoord[j]);
                    int dy = Math.Abs(yCoord[i] - yCoord[j]);
                    
                    // Sąsiedzi w siatce (poziomo, pionowo lub po przekątnej)
                    if ((dx <= 1 && dy <= 1) && random.NextDouble() < edgeProbability)
                    {
                        graph.AddEdge(i, j);
                    }
                }
            }
            
            return graph;
        }

        // Zapisz graf do pliku
        public void SaveGraphToFile(Graph graph, string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(graph.VertexCount);
                
                for (int i = 0; i < graph.VertexCount; i++)
                {
                    for (int j = i + 1; j < graph.VertexCount; j++)
                    {
                        if (graph.AreAdjacent(i, j))
                        {
                            writer.WriteLine($"{i} {j}");
                        }
                    }
                }
            }
        }

        // Wczytaj graf z pliku
        public static Graph LoadGraphFromFile(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                int vertexCount = int.Parse(reader.ReadLine());
                Graph graph = new Graph(vertexCount);
                
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(' ');
                    int v1 = int.Parse(parts[0]);
                    int v2 = int.Parse(parts[1]);
                    graph.AddEdge(v1, v2);
                }
                
                return graph;
            }
        }
    }

    /// <summary>
    /// Klasa reprezentująca osobnika w algorytmie genetycznym
    /// </summary>
    class Individual
    {
        public int[] Coloring { get; private set; }
        public int ColorCount { get; private set; }
        public int Fitness { get; private set; }
        private Graph graph;

        public Individual(Graph graph, int maxColors, Random random)
        {
            this.graph = graph;
            Coloring = new int[graph.VertexCount];
            
            // Losowe przypisanie kolorów
            for (int i = 0; i < graph.VertexCount; i++)
            {
                Coloring[i] = random.Next(maxColors);
            }
            
            EvaluateFitness();
        }

        public Individual(Graph graph, int[] coloring)
        {
            this.graph = graph;
            Coloring = (int[])coloring.Clone();
            EvaluateFitness();
        }

        // Oblicza przystosowanie osobnika (mniej konfliktów = lepsze przystosowanie)
        private void EvaluateFitness()
        {
            int conflicts = 0;
            HashSet<int> usedColors = new HashSet<int>();
            
            for (int i = 0; i < graph.VertexCount; i++)
            {
                usedColors.Add(Coloring[i]);
                
                for (int j = i + 1; j < graph.VertexCount; j++)
                {
                    if (graph.AreAdjacent(i, j) && Coloring[i] == Coloring[j])
                    {
                        conflicts++;
                    }
                }
            }
            
            ColorCount = usedColors.Count;
            
            // Funkcja przystosowania: kara za konflikty i za liczbę kolorów
            Fitness = -(conflicts * 100 + ColorCount);
        }

        // Sprawdza, czy kolorowanie jest poprawne (brak konfliktów)
        public bool IsValid()
        {
            for (int i = 0; i < graph.VertexCount; i++)
            {
                for (int j = i + 1; j < graph.VertexCount; j++)
                {
                    if (graph.AreAdjacent(i, j) && Coloring[i] == Coloring[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Mutacja: zamiana koloru losowego wierzchołka
        public void Mutate(int maxColors, Random random)
        {
            int vertex = random.Next(graph.VertexCount);
            int newColor = random.Next(maxColors);
            Coloring[vertex] = newColor;
            EvaluateFitness();
        }

        // Krzyżowanie z innym osobnikiem
        public Individual Crossover(Individual other, Random random)
        {
            int[] childColoring = new int[graph.VertexCount];
            
            // Punkt krzyżowania
            int crossPoint = random.Next(1, graph.VertexCount);
            
            for (int i = 0; i < crossPoint; i++)
            {
                childColoring[i] = Coloring[i];
            }
            
            for (int i = crossPoint; i < graph.VertexCount; i++)
            {
                childColoring[i] = other.Coloring[i];
            }
            
            return new Individual(graph, childColoring);
        }

        // Optymalizacja lokalna - próba zmniejszenia liczby kolorów
        public void LocalOptimization(Random random)
        {
            // Sprawdź, czy możemy zredukować liczbę kolorów
            if (!IsValid()) return; // Nie optymalizujemy niepoprawnych rozwiązań
            
            int maxColorUsed = Coloring.Max();
            
            for (int color = maxColorUsed; color >= 1; color--)
            {
                // Zbierz wszystkie wierzchołki z danym kolorem
                List<int> verticesWithColor = new List<int>();
                for (int i = 0; i < graph.VertexCount; i++)
                {
                    if (Coloring[i] == color)
                    {
                        verticesWithColor.Add(i);
                    }
                }
                
                // Dla każdego takiego wierzchołka próbujemy przypisać mniejszy kolor
                foreach (int vertex in verticesWithColor)
                {
                    int originalColor = Coloring[vertex];
                    
                    // Sprawdzamy wszystkie mniejsze kolory
                    for (int newColor = 0; newColor < color; newColor++)
                    {
                        Coloring[vertex] = newColor;
                        bool conflict = false;
                        
                        // Sprawdzamy, czy nie ma konfliktu
                        for (int j = 0; j < graph.VertexCount; j++)
                        {
                            if (j != vertex && graph.AreAdjacent(vertex, j) && Coloring[j] == newColor)
                            {
                                conflict = true;
                                break;
                            }
                        }
                        
                        if (!conflict)
                        {
                            // Znaleziono lepszy kolor, zostawiamy go
                            break;
                        }
                        else
                        {
                            // Przywracamy oryginalny kolor
                            Coloring[vertex] = originalColor;
                        }
                    }
                }
            }
            
            EvaluateFitness();
        }

        public override string ToString()
        {
            return $"Liczba kolorów: {ColorCount}, Fitness: {Fitness}, Poprawne: {IsValid()}";
        }
    }

    /// <summary>
    /// Główna klasa algorytmu genetycznego do kolorowania grafu
    /// </summary>
    class GeneticAlgorithm
    {
        private Graph graph;
        private int populationSize;
        private int maxGenerations;
        private double mutationRate;
        private double crossoverRate;
        private int tournamentSize;
        private int maxColors;
        private Random random;
        
        private List<Individual> population;
        private Individual bestIndividual;

        public GeneticAlgorithm(
            Graph graph, 
            int populationSize = 100, 
            int maxGenerations = 1000,
            double mutationRate = 0.1, 
            double crossoverRate = 0.8,
            int tournamentSize = 5,
            int maxColors = 10,
            int seed = 0)
        {
            this.graph = graph;
            this.populationSize = populationSize;
            this.maxGenerations = maxGenerations;
            this.mutationRate = mutationRate;
            this.crossoverRate = crossoverRate;
            this.tournamentSize = tournamentSize;
            this.maxColors = maxColors;
            this.random = seed == 0 ? new Random() : new Random(seed);
            
            population = new List<Individual>();
        }

        // Inicjalizacja początkowej populacji
        private void InitializePopulation()
        {
            population.Clear();
            
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(new Individual(graph, maxColors, random));
            }
            
            UpdateBestIndividual();
        }

        // Selekcja turniejowa
        private Individual TournamentSelection()
        {
            Individual best = null;
            
            for (int i = 0; i < tournamentSize; i++)
            {
                int idx = random.Next(population.Count);
                Individual contestant = population[idx];
                
                if (best == null || contestant.Fitness > best.Fitness)
                {
                    best = contestant;
                }
            }
            
            return best;
        }

        // Aktualizacja najlepszego osobnika
        private void UpdateBestIndividual()
        {
            Individual currentBest = population.OrderByDescending(ind => ind.Fitness).First();
            
            if (bestIndividual == null || currentBest.Fitness > bestIndividual.Fitness)
            {
                bestIndividual = new Individual(graph, currentBest.Coloring);
            }
        }

        // Główna metoda algorytmu genetycznego
        public Individual Solve(bool printProgress = true, int progressInterval = 100)
        {
            InitializePopulation();
            
            for (int generation = 0; generation < maxGenerations; generation++)
            {
                // Tworzenie nowej populacji
                List<Individual> newPopulation = new List<Individual>();
                
                while (newPopulation.Count < populationSize)
                {
                    // Selekcja rodziców
                    Individual parent1 = TournamentSelection();
                    Individual parent2 = TournamentSelection();
                    
                    Individual child;
                    
                    // Krzyżowanie
                    if (random.NextDouble() < crossoverRate)
                    {
                        child = parent1.Crossover(parent2, random);
                    }
                    else
                    {
                        // Kopiowanie jednego z rodziców
                        child = new Individual(graph, random.Next(2) == 0 ? parent1.Coloring : parent2.Coloring);
                    }
                    
                    // Mutacja
                    if (random.NextDouble() < mutationRate)
                    {
                        child.Mutate(maxColors, random);
                    }
                    
                    // Optymalizacja lokalna (z małym prawdopodobieństwem)
                    if (random.NextDouble() < 0.1)
                    {
                        child.LocalOptimization(random);
                    }
                    
                    newPopulation.Add(child);
                }
                
                // Zastępowanie starej populacji
                population = newPopulation;
                
                // Aktualizacja najlepszego osobnika
                UpdateBestIndividual();
                
                // Wyświetlanie postępu
                if (printProgress && generation % progressInterval == 0)
                {
                    Console.WriteLine($"Generacja {generation}: {bestIndividual}");
                }
                
                // Warunek wcześniejszego zakończenia: znaleziono poprawne rozwiązanie
                if (bestIndividual.IsValid() && generation > maxGenerations / 10)
                {
                    Console.WriteLine($"Znaleziono poprawne rozwiązanie w generacji {generation}");
                    break;
                }
            }
            
            // Optymalizacja końcowa dla najlepszego osobnika
            bestIndividual.LocalOptimization(random);
            
            return bestIndividual;
        }
    }

    /// <summary>
    /// Klasa eksperymentów do badania efektywności algorytmu
    /// </summary>
    class ExperimentRunner
    {
        // Uruchamia eksperymenty dla różnych parametrów i instancji
        public static void RunExperiments()
        {
            Console.WriteLine("=== EKSPERYMENTY Z ALGORYTMEM GENETYCZNYM DO KOLOROWANIA GRAFU ===");
            
            // Eksperymenty z różnymi typami grafów
            RunGraphTypeExperiments();
            
            // Eksperymenty z różnymi parametrami algorytmu
            RunParameterExperiments();
        }

        // Eksperymenty z różnymi typami grafów
        private static void RunGraphTypeExperiments()
        {
            Console.WriteLine("\n=== EKSPERYMENTY Z RÓŻNYMI TYPAMI GRAFÓW ===");
            
            GraphGenerator generator = new GraphGenerator(42); // Stały seed dla powtarzalności
            
            // Test dla grafu losowego o różnej gęstości
            int[] vertexCounts = { 20, 50, 100 };
            double[] densities = { 0.1, 0.3, 0.5, 0.7 };
            
            Console.WriteLine("\n--- GRAFY LOSOWE ---");
            foreach (int vertexCount in vertexCounts)
            {
                foreach (double density in densities)
                {
                    Console.WriteLine($"\nGraf losowy: {vertexCount} wierzchołków, gęstość: {density}");
                    Graph graph = generator.GenerateRandomGraph(vertexCount, density);
                    
                    GeneticAlgorithm ga = new GeneticAlgorithm(
                        graph, 
                        populationSize: 100, 
                        maxGenerations: 500,
                        maxColors: vertexCount / 3);
                    
                    Individual solution = ga.Solve(printProgress: false);
                    Console.WriteLine($"Wynik: {solution}");
                }
            }
            
            // Test dla grafu k-regularnego
            int[] regularities = { 3, 4, 5 };
            
            Console.WriteLine("\n--- GRAFY K-REGULARNE ---");
            foreach (int vertexCount in vertexCounts.Where(v => v >= 10))
            {
                foreach (int k in regularities.Where(k => k < vertexCount && (k * vertexCount) % 2 == 0))
                {
                    Console.WriteLine($"\nGraf {k}-regularny: {vertexCount} wierzchołków");
                    try
                    {
                        Graph graph = generator.GenerateRegularGraph(vertexCount, k);
                        
                        GeneticAlgorithm ga = new GeneticAlgorithm(
                            graph, 
                            populationSize: 100, 
                            maxGenerations: 500,
                            maxColors: k + 1);
                        
                        Individual solution = ga.Solve(printProgress: false);
                        Console.WriteLine($"Wynik: {solution}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd: {ex.Message}");
                    }
                }
            }
            
            // Test dla grafu planarnego
            double[] edgeProbabilities = { 0.3, 0.6, 1.0 };
            
            Console.WriteLine("\n--- GRAFY PLANARNE ---");
            foreach (int vertexCount in vertexCounts)
            {
                foreach (double prob in edgeProbabilities)
                {
                    Console.WriteLine($"\nGraf planarny: {vertexCount} wierzchołków, prawdopodobieństwo krawędzi: {prob}");
                    Graph graph = generator.GeneratePlanarGraph(vertexCount, prob);
                    
                    GeneticAlgorithm ga = new GeneticAlgorithm(
                        graph, 
                        populationSize: 100, 
                        maxGenerations: 500,
                        maxColors: 6);  // Graf planarny można pokolorować używając maksymalnie 6 kolorów
                    
                    Individual solution = ga.Solve(printProgress: false);
                    Console.WriteLine($"Wynik: {solution}");
                }
            }
        }

        // Eksperymenty z różnymi parametrami algorytmu
        private static void RunParameterExperiments()
        {
            Console.WriteLine("\n=== EKSPERYMENTY Z RÓŻNYMI PARAMETRAMI ALGORYTMU ===");
            
            GraphGenerator generator = new GraphGenerator(42);
            Graph graph = generator.GenerateRandomGraph(50, 0.3);
            
            // Test różnych wielkości populacji
            int[] populationSizes = { 20, 50, 100, 200 };
            
            Console.WriteLine("\n--- RÓŻNE WIELKOŚCI POPULACJI ---");
            foreach (int popSize in populationSizes)
            {
                Console.WriteLine($"\nWielkość populacji: {popSize}");
                GeneticAlgorithm ga = new GeneticAlgorithm(
                    graph, 
                    populationSize: popSize, 
                    maxGenerations: 300,
                    maxColors: 15);
                
                Individual solution = ga.Solve(printProgress: false);
                Console.WriteLine($"Wynik: {solution}");
            }
            
            // Test różnych współczynników mutacji
            double[] mutationRates = { 0.01, 0.05, 0.1, 0.2 };
            
            Console.WriteLine("\n--- RÓŻNE WSPÓŁCZYNNIKI MUTACJI ---");
            foreach (double mutationRate in mutationRates)
            {
                Console.WriteLine($"\nWspółczynnik mutacji: {mutationRate}");
                GeneticAlgorithm ga = new GeneticAlgorithm(
                    graph, 
                    populationSize: 100, 
                    maxGenerations: 300,
                    mutationRate: mutationRate,
                    maxColors: 15);
                
                Individual solution = ga.Solve(printProgress: false);
                Console.WriteLine($"Wynik: {solution}");
            }
            
            // Test różnych współczynników krzyżowania
            double[] crossoverRates = { 0.6, 0.7, 0.8, 0.9 };
            
            Console.WriteLine("\n--- RÓŻNE WSPÓŁCZYNNIKI KRZYŻOWANIA ---");
            foreach (double crossoverRate in crossoverRates)
            {
                Console.WriteLine($"\nWspółczynnik krzyżowania: {crossoverRate}");
                GeneticAlgorithm ga = new GeneticAlgorithm(
                    graph, 
                    populationSize: 100, 
                    maxGenerations: 300,
                    crossoverRate: crossoverRate,
                    maxColors: 15);
                
                Individual solution = ga.Solve(printProgress: false);
                Console.WriteLine($"Wynik: {solution}");
            }
            
            // Test różnych wielkości turnieju
            int[] tournamentSizes = { 2, 3, 5, 8 };
            
            Console.WriteLine("\n--- RÓŻNE WIELKOŚCI TURNIEJU ---");
            foreach (int tournamentSize in tournamentSizes)
            {
                Console.WriteLine($"\nWielkość turnieju: {tournamentSize}");
                GeneticAlgorithm ga = new GeneticAlgorithm(
                    graph, 
                    populationSize: 100, 
                    maxGenerations: 300,
                    tournamentSize: tournamentSize,
                    maxColors: 15);
                
                Individual solution = ga.Solve(printProgress: false);
                Console.WriteLine($"Wynik: {solution}");
            }
        }
    }

    /// <summary>
    /// Główny program demonstracyjny
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PROGRAM DO KOLOROWANIA GRAFU PRZY UŻYCIU ALGORYTMU GENETYCZNEGO");
            Console.WriteLine("===========================================================");
            
            // Przykładowe użycie programu
            DemoUsage();
            
            // Uruchomienie eksperymentów
            // ExperimentRunner.RunExperiments();
            
            Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
            Console.ReadKey();
        }

        // Demonstracja użycia programu
        static void DemoUsage()
        {
            Console.WriteLine("\nDEMONSTRACJA UŻYCIA PROGRAMU:");
            
            // Generowanie grafu
            Console.WriteLine("\n1. Generowanie przykładowego grafu");
            GraphGenerator generator = new GraphGenerator();
            Graph graph = generator.GenerateRandomGraph(20, 0.3);
            Console.WriteLine(graph);
            
            // Zapis i odczyt grafu
            Console.WriteLine("\n2. Zapis grafu do pliku i odczyt");
            string filename = "example_graph.txt";
            generator.SaveGraphToFile(graph, filename);
            Console.WriteLine($"Graf zapisany do pliku {filename}");
            
            Graph loadedGraph = GraphGenerator.LoadGraphFromFile(filename);
            Console.WriteLine("Graf odczytany z pliku:");
            Console.WriteLine(loadedGraph);
            
            // Rozwiązanie problemu kolorowania grafu
            Console.WriteLine("\n3. Rozwiązanie problemu kolorowania grafu");
            GeneticAlgorithm ga = new GeneticAlgorithm(
                graph, 
                populationSize: 100, 
                maxGenerations: 200,
                mutationRate: 0.1,
                crossoverRate: 0.8,
                tournamentSize: 5,
                maxColors: 10);
            
            Individual solution = ga.Solve(progressInterval: 20);
            
            // Wyświetlenie wyniku
            Console.WriteLine("\n4. Wynik końcowy:");
            Console.WriteLine(solution);
            Console.WriteLine("\nKolorowanie wierzchołków:");
            for (int i = 0; i < graph.VertexCount; i++)
            {
                Console.WriteLine($"Wierzchołek {i}: kolor {solution.Coloring[i]}");
            }
            
            if (solution.IsValid())
            {
                Console.WriteLine("\nKolorowanie jest poprawne!");
            }
            else
            {
                Console.WriteLine("\nKolorowanie zawiera konflikty!");
            }
        }
    }
}