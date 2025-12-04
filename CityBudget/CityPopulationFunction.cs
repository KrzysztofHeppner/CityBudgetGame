using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBudget
{
    class CityPopulationFunction
    {
        private static Random rand = new Random();

        public static Person[] MakeNewPoplation(
            uint numberOfCitizens,
            float averageZadowolenie,
            float averageWiek,
            float averageDzieci,
            float averageStres,
            float averagePary,
            float averageWyksztalcenie,
            float averageMajatek)
        {
            Person[] population = new Person[numberOfCitizens];
            List<Person> singles = new List<Person>();

            for (uint i = 0; i < numberOfCitizens; i++)
            {
                uint id = i + 1;
                bool plec = rand.NextDouble() >= 0.5;

                sbyte wiek = (sbyte)Math.Clamp(NextGaussian(averageWiek, 18), 0, 110);

                sbyte wyksztalcenie = (sbyte)Math.Clamp(Math.Round(NextGaussian(averageWyksztalcenie, 0.8)), 0, 3);

                sbyte stres = (sbyte)Math.Clamp(Math.Round(NextGaussian(averageStres, 0.8)), 0, 3);
                sbyte zadowolenie = (sbyte)Math.Clamp(Math.Round(NextGaussian(averageZadowolenie, 0.8)), 0, 3);

                sbyte iloscDzieci = (sbyte)Math.Max(0, Math.Round(NextGaussian(averageDzieci, 1.0)));
                if (wiek < 18) iloscDzieci = 0;
                bool posiadaDzieci = iloscDzieci > 0;

                float majatek = (float)Math.Max(0, NextGaussian(averageMajatek, averageMajatek * 0.5f));

                float zdrowie = Math.Clamp(100f - (wiek * 0.5f) + (float)NextGaussian(0, 10), 0, 100);

                bool student = false;
                bool emerytura = false;
                bool praca = false;
                bool szukaPracy = false;
                sbyte lataPracy = 0;

                if (wiek >= 65)
                {
                    emerytura = true;
                    lataPracy = (sbyte)(wiek - 20);
                }
                else if (wiek >= 19 && wiek < 25 && wyksztalcenie > 1)
                {
                    student = rand.NextDouble() > 0.3;
                }

                if (!student && !emerytura && wiek >= 18)
                {
                    lataPracy = (sbyte)(wiek - 18);
                    praca = rand.NextDouble() > 0.1;
                    szukaPracy = !praca;
                }

                Person p = new Person(
                    id, plec, 0, wiek, praca, zdrowie, majatek,
                    szukaPracy, lataPracy, emerytura, student, posiadaDzieci,
                    iloscDzieci, wyksztalcenie, stres, zadowolenie
                );

                population[i] = p;

                if (wiek >= 18)
                {
                    singles.Add(p);
                }
            }

            int targetCouplesCount = (int)((singles.Count * Math.Clamp(averagePary, 0f, 1f)) / 2);

            singles = singles.OrderBy(x => rand.Next()).ToList();

            int pairsCreated = 0;

            for (int i = 0; i < singles.Count - 1; i += 2)
            {
                if (pairsCreated >= targetCouplesCount) break;

                Person p1 = singles[i];
                Person p2 = singles[i + 1];

                p1.paraId = p2.id;
                p2.paraId = p1.id;

                pairsCreated++;
            }

            return population;
        }

        private static double NextGaussian(double mean, double stdDev)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}
