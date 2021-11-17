using DI;
using System.Diagnostics;


// Create a service list
var italianHouseList = new ServiceList();

// Request service
italianHouseList.Add<Pizza>();

// Services can implement interfaces or base classes
italianHouseList.Add<ItalianParent>().Is<IPerson>();
italianHouseList.Add<ItalianHouse>().Is<House>();

// One base class and multiple interfaces can be implemented by the same service
italianHouseList.Add<ItalianChild>().Is<Child>().Is<IPerson>();

// Services can depend on an array of interfaces (or base classes)
// Upon creation, the array contains the services present in the same list that implement the interface.
italianHouseList.Add<ItalianFamily>();                   

// Multiple service lists can be created
var englishHouseList = new ServiceList();
englishHouseList.Add<EnglishHouse>().Is<House>();
englishHouseList.Add<EnglishFamily>();
englishHouseList.Add<EnglishChild>().Is<IPerson>();

// Externally-created services can be added
var foreignStudent = new ForeignStudent();
englishHouseList.Add(foreignStudent).Is<IPerson>();

// Lists can be nested.
// Each list must specifies its root service, which is the only service visible outside.
var houseList = new ServiceList();
houseList.Add<ItalianHouse>(italianHouseList);
houseList.Add<EnglishHouse>(englishHouseList);

// The same service can be added to multiple sublists, as long as its dependencies are not interface (or base class)
var peopleList = new ServiceList();
peopleList.Add<ItalianChild>().Is<IPerson>();
peopleList.Add<ItalianParent>().Is<IPerson>();
peopleList.Add<EnglishChild>().Is<IPerson>();
peopleList.Add<Census>();
peopleList.Add<People>();

// It is ok to add an external service to multiple sublists, 
// as long as the object is the same, and not another instance.
peopleList.Add(foreignStudent);

// Sublists are added to final list
var finalList = new ServiceList();
finalList.Add<ItalianHouse>(italianHouseList);
finalList.Add<EnglishHouse>(englishHouseList);
finalList.Add<Census>(peopleList);

// A service can depend on an interface (or base class),
// as long as a service is the same list implements it.
finalList.Add<Village>();
finalList.Add<Winter>().Is<Season>();


// Create the dependency injection container
var container = new Container(finalList);

// Get a service
var village = container.Get<Village>();
var italianFamily = container.Get<ItalianFamily>();
var englishFamily = container.Get<EnglishFamily>();
var people = container.Get<People>();

// Get all services that implement an interface (or base class)
container.Get<House>(out var houses);
container.Get<Season>(out var seasons);

// Checks
Debug.Assert(houses.Length == 2);
Debug.Assert(seasons.Length == 1);
Debug.Assert(village.houseCnt == 2);
Debug.Assert(village.seasonName == "Winter");
Debug.Assert(italianFamily.cnt == 2);
Debug.Assert(englishFamily.cnt == 2);
Debug.Assert(people.cnt == 4);
