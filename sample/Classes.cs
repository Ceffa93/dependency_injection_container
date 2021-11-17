class Pizza { }

interface IPerson { }
abstract class Child { }

class ItalianChild : Child, IPerson  { public ItalianChild(Pizza pizza) { } }
class ItalianParent : IPerson { }
class EnglishChild : Child, IPerson { }
class ForeignStudent : IPerson { }

class ItalianFamily {
    public ItalianFamily(Pizza pizza, IPerson[] members) => cnt = members.Length;
    public int cnt;
};
class EnglishFamily
{
    public EnglishFamily(IPerson[] members) => cnt = members.Length;
    public int cnt;
};

abstract class House { }

class ItalianHouse : House { public ItalianHouse(ItalianFamily family) { } }
class EnglishHouse : House { public EnglishHouse(EnglishFamily family) { } }

class People { 
    public People(IPerson[] people) => cnt = people.Length;
    public int cnt;
}

class Census { public Census(People people) { } }

class Season { 
    protected Season(string name) { 
        this.name = name; 
    }
    public string name;
}

class Winter : Season { public Winter() : base("Winter") { } }

class Village
{
    public Village(Census census, House[] houses, Season season)
    {
        houseCnt = houses.Length;
        seasonName = season.name;
    }
    public int houseCnt;
    public string seasonName;
}



