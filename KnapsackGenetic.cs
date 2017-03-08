using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Genetic_Knapsack
{
    public struct item
    {
        public string name;
        public int cost;
        public int value;
    };

    class Sack
    {
        //variables
        private string itemList;
        private int sumCost;
        private int sumVal;
        private int capacity;
        private int fitness;

        public Sack(List<item> collection, int c, Random rng)
        {
            StringBuilder check = new StringBuilder();
            sumCost = 0;
            sumVal = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                if (rng.Next(0, 2) == 1)
                {
                    sumCost += collection[i].cost;
                    sumVal += collection[i].value;
                    check.Append("1");
                }
                else
                {
                    check.Append("0");
                }
            }
            if (sumCost > c)
                fitness = 0;
            else
                fitness = sumVal;
            capacity = c;
            itemList = check.ToString();
        }
        public Sack(string stuff, List<item> collection, int c)
        {
            itemList = stuff;
            sumCost = 0;
            sumVal = 0;
            for(int i=0; i<stuff.Length; i++)
            {
                if(stuff[i] == '1')
                {
                    sumCost += collection[i].cost;
                    sumVal += collection[i].value;
                }
            }
            if (sumCost > c)
                fitness = 0;
            else
                fitness = sumVal;
            capacity = c;
        }
        public void display()
        {
            Console.WriteLine("Sack: {0}", itemList);
            Console.WriteLine("Sum of Costs: {0}", sumCost);
            Console.WriteLine("Sum of Values: {0}", sumVal);
        }
        public string getSack()
        {
            return itemList;
        }
        public int getVal()
        {
            return sumVal;
        }
        public int getCost()
        {
            return sumCost;
        }
        public int getFitness()
        {
            return fitness;
        }
        public int getCap()
        {
            return capacity;
        }
        public void setSack(string x, List<item> stuff)
        {
            itemList = x;
            sumCost = 0;
            sumVal = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] == '1')
                {
                    sumCost += stuff[i].cost;
                    sumVal += stuff[i].value;
                }
            }
            if (sumCost > capacity)
                fitness = 0;
            else
                fitness = sumVal;
        }
    }

    class Program
    {
        static Sack crossover(Sack parent1, Sack parent2, List<item> stuff, Random rng)
        {
            StringBuilder p1 = new StringBuilder(parent1.getSack());
            StringBuilder p2 = new StringBuilder(parent2.getSack());
            StringBuilder child = new StringBuilder();
            int grab = rng.Next(0, p1.Length);
            for (int i = 0; i < p1.Length; i++)
            {
                bool mutate = false;
                if (rng.NextDouble() <= .0005)
                    mutate = true;
                if (i < grab)
                    child.Append(p1[i]);
                else
                    child.Append(p2[i]);
                if(mutate)
                {
                    if (child[i] == '1')
                        child[i] = '0';
                    else
                        child[i] = '1';
                }
            }
            return new Sack(child.ToString(), stuff, parent1.getCap());
        }
        static int roulette(List<Sack> population, ref int count, Random rng)
        {
            int fitsum = 0;
            foreach (var obj in population)
            {
                fitsum += obj.getFitness();
                count++;
            }
            double value = rng.NextDouble() * fitsum;
            for (int i = 0; i < population.Count; i++)
            {
                value -= population[i].getFitness();
                count++;
                if (value <= 0)
                    return i;
            }
            return population.Count - 1;
        }
        static void cataclysm(ref List<Sack> population, Random rng, List<item> stuff)
        {
            for(int i=1; i<population.Count; i++)
            {
                StringBuilder next = new StringBuilder(population[i].getSack());
                for(int j=0; j<next.Length; j++)
                {
                    bool mutate = false;
                    if (rng.NextDouble() <= .2)
                        mutate = true;
                    if(mutate)
                    {
                        if (next[j] == '1')
                            next[j] = '0';
                        else
                            next[j] = '1';
                    }
                }
                population[i].setSack(next.ToString(), stuff);
            }
            return;
        }
        static Sack genetic(List<Sack> population, Stopwatch time, List<item> stuff, Random rng)
        {
            time.Start();
            int fitnessCount = 0;
            int convergeCount = 0;
            string convergeList = "";
            Sack best = null;
            int count = 0;
            while(convergeCount < 3 && time.ElapsedMilliseconds < 600000)
            {
                count++;
                population.Sort((x, y) => y.getFitness().CompareTo(x.getFitness()));
                fitnessCount += 100;
                int parent1 = roulette(population, ref fitnessCount, rng);
                int parent2 = roulette(population, ref fitnessCount, rng);
                while (parent1 == parent2)
                    parent2 = roulette(population, ref fitnessCount, rng);
                Sack child = crossover(population[parent1], population[parent2], stuff, rng);
                if (child.getFitness() > population[population.Count-1].getFitness())
                {
                    population.Remove(population[population.Count-1]);
                    population.Add(child);
                }
                fitnessCount += 2;

                //check if cataclysmic mutation is needed
                bool same = false;
                string check = population[0].getSack();
                if(check == population[population.Count-1].getSack())
                {
                    same = true;
                    foreach(var obj in population)
                    {
                        if (check != obj.getSack())
                            same = false;
                    }
                }
                //if everything is the same, perform cataclysmic mutation.
                if (same)
                {
                    cataclysm(ref population, rng, stuff);
                    if (check == convergeList)
                        convergeCount++;
                    else
                    {
                        convergeCount = 1;
                        convergeList = check;
                        best = population[0];
                    }
                }
            }
            time.Stop();
            Console.WriteLine("Fitness Comparison Count: {0}", fitnessCount);
            if (convergeCount == 3)
                return best;
            return population[0];
        }

        static void print(List<item> collection)
        {
            var sum = 0;
            var cost = 0;
            Console.WriteLine("{0,-15}{1,-15}{2,-15}{3,-15}", "Item Name", "Item Cost", "Item Value", "Item Ratio");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------");
            foreach (var obj in collection)
            {
                Console.WriteLine("{0,-15}{1,-15}{2,-15}{3,-15}", obj.name, obj.cost, obj.value);
                sum += obj.value;
                cost += obj.cost;
            }
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("Sum of Values: {0}", sum);
            Console.WriteLine("Sum of Costs: {0}\n", cost);
        }

        static void Main(string[] args)
        {
            //Get file name and create filestream
            Console.Write("Input filename: ");
            var filename = Console.ReadLine();
            StreamReader file = new StreamReader(filename);

            //create the item list
            List<item> collection = new List<item>();
            var capacity = Convert.ToInt32(file.ReadLine());
            var newItem = file.ReadLine();
            while (newItem != null)
            {
                var thing = newItem.Split(',');
                item temp;
                temp.name = thing[0];
                temp.cost = Convert.ToInt32(thing[1]);
                temp.value = Convert.ToInt32(thing[2]);
                collection.Add(temp);
                newItem = file.ReadLine();
            }
            Random rng = new Random();
            List<Sack> population = new List<Sack>();
            for (int i = 0; i < 100; i++) 
                population.Add(new Sack(collection, capacity, rng));
            Stopwatch timer = new Stopwatch();
            Sack solution = genetic(population, timer, collection, rng);
            Console.WriteLine("Solution: {0}", solution.getSack());
            Console.WriteLine("Sum of Values: {0}", solution.getVal());
            Console.WriteLine("Sum of Costs: {0}", solution.getCost());
            Console.WriteLine("Capacity: {0}", capacity);
            Console.WriteLine("Time Taken: {0}", timer.Elapsed);
        }
    }
}