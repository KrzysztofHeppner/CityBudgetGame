using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBudget
{
    public class Person
    {
        public uint id;
        public bool plec; // false - kobieta, true - mezczyzna
        public uint paraId; // 0 - brak pary
        public sbyte wiek;
        public bool praca;
        public bool zyje;
        public float zdrowie;
        public float majatek;
        public bool szukaPracy;
        public sbyte lataPracy;
        public bool emerytura;
        public bool student;
        public bool posiadaDzieci;
        public sbyte iloscDzieci;
        public sbyte wyksztalcenie; // 0-brak, 1-podstawowe, 2-srednie, 3-wyzsze
        public sbyte poziomStresu; // 0-brak, 1-niski, 2-sredni, 3-wysoki
        public sbyte poziomZadowolenia; // 0-brak, 1-niski, 2-sredni, 3-wysoki

        public Person()
        {
            zyje = true;
        }

        public Person(uint id, bool plec, uint paraId, sbyte wiek, bool praca, float zdrowie, float majatek, 
                      bool szukaPracy, sbyte lataPracy, bool emerytura, bool student, bool posiadaDzieci,
                      sbyte iloscDzieci, sbyte wyksztalcenie, sbyte poziomStresu, sbyte poziomZadowolenia)
        {
            zyje = true;
            this.id = id;
            this.plec = plec;
            this.wiek = wiek;
            this.praca = praca;
            this.zdrowie = zdrowie;
            this.majatek = majatek;
            this.szukaPracy = szukaPracy;
            this.lataPracy = lataPracy;
            this.emerytura = emerytura;
            this.student = student;
            this.posiadaDzieci = posiadaDzieci;
            this.iloscDzieci = iloscDzieci;
            this.wyksztalcenie = wyksztalcenie;
            this.poziomStresu = poziomStresu;
            this.poziomZadowolenia = poziomZadowolenia;
        }


    }
}
