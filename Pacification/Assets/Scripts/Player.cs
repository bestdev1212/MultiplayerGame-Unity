﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Player
{
    public HexGrid hexGrid;
    public Client client;

    const int MaxIterationGen = 1000;
    const int MaxUnitInitSpawnRadius = 10;

    private int[] unitLevel;

    public int money;
    public int science;
    public int[] resources;

    public List<Unit> playerUnits;
    public List<City> playerCities;
    public List<Resource> playerResources;

    // TODO : couleur du joueur

    public bool canPlay;
    public string name;
    private int roundNb;

    public DisplayInformationManager displayer;

    public Player(string name)
    {
        this.name = name;
        unitLevel = new int[] {1, 1, 1};

        money = 1000;
        science = 0;
        resources = new int[6]; // Iron, gold, Diamond, Horses, Wood, Food

        roundNb = 0;

        playerUnits = new List<Unit>();
        playerCities = new List<City>();
        playerResources = new List<Resource>();

        canPlay = false;
        client = Object.FindObjectOfType<Client>();
        hexGrid = Object.FindObjectOfType<HexGrid>();
    }

    public void InitialSpawnUnit()
    {
        displayer = Object.FindObjectOfType<DisplayInformationManager>();
        hexGrid = Object.FindObjectOfType<HexGrid>();

        List<HexCell> possibleLocation = new List<HexCell>();
        for(int i = 0; i < hexGrid.cells.Length; ++i)
        {
            HexCell cell = hexGrid.cells[i];
            if(!cell.IsUnderWater && !cell.Unit && !hexGrid.IsBorder(cell) && cell.Elevation <= 4)
                possibleLocation.Add(cell);
        }

        HexCell randomCell = null;
        int guard = 0;
        do
        {
            int rnd = hexGrid.rnd.Next(possibleLocation.Count);
            randomCell = possibleLocation[rnd];
            possibleLocation.RemoveAt(rnd);
            guard++;
        } while(possibleLocation.Count > 0 && guard < MaxIterationGen &&
                hexGrid.OtherUnitInRadius(randomCell, MaxUnitInitSpawnRadius));

        if(randomCell == null || possibleLocation.Count == 0 || guard == MaxIterationGen ||
            hexGrid.OtherUnitInRadius(randomCell, MaxUnitInitSpawnRadius))
            Debug.LogError("The current map is too small for this many players");

        HexCell spawnSettler = randomCell;
        HexCell spawnAttacker = hexGrid.GetNearFreeCell(randomCell);

        AddUnits(Unit.UnitType.SETTLER, spawnSettler, Unit.UnitType.REGULAR, spawnAttacker);

        HexMapCamera.FocusOnPosition(spawnSettler.Position);
    }

    public void AddUnit(Unit.UnitType type, HexCell location)
    {
        client.Send("CUNI|UNC|" + (int)type + "#" + location.coordinates.X + "#" + location.coordinates.Z + "#" + GetUnitLevel(type));
    }

    public void AddUnits(Unit.UnitType type, HexCell location, Unit.UnitType type2, HexCell location2)
    {
        client.Send("CUNM|UAA|" + (int)type + "#" + location.coordinates.X + "#" + location.coordinates.Z + "#" + GetUnitLevel(type) + "|" + (int)type2 + "#" + location2.coordinates.X + "#" + location2.coordinates.Z + "#" + GetUnitLevel(type2));
    }

    public void NetworkAddUnit(string data)
    {
        string[] receivedData = data.Split('#');
        hexGrid = Object.FindObjectOfType<HexGrid>();

        Unit.UnitType type = (Unit.UnitType)int.Parse(receivedData[0]);
        HexCell location = hexGrid.GetCell(new HexCoordinates(int.Parse(receivedData[1]), int.Parse(receivedData[2])));
        Unit unit = null;

        if(type == Unit.UnitType.SETTLER)
            unit = new Settler(this);
        else if(type == Unit.UnitType.WORKER)
            unit = new Worker(this);
        else if(type == Unit.UnitType.REGULAR)
        {
            Regular regular = new Regular(this);
            regular.Level = int.Parse(receivedData[3]);
            if(regular.Level >= 10)
                type = (Unit.UnitType)((int)type + 4);
            unit = regular;
        }
        else if(type == Unit.UnitType.RANGED)
        {
            Ranged ranged = new Ranged(this);
            ranged.Level = int.Parse(receivedData[3]);
            if(ranged.Level >= 10)
                type = (Unit.UnitType)((int)type + 4);
            unit = ranged;
        }
        else if(type == Unit.UnitType.HEAVY)
        {
            Heavy heavy = new Heavy(this);
            heavy.Level = int.Parse(receivedData[3]);
            if(heavy.Level >= 10)
                type = (Unit.UnitType)((int)type + 4);
            unit = heavy;
        }
        else
            Debug.Log("Unknown unit type");

        unit.hexGameObject = GameObject.Instantiate(hexGrid.mainUnitPrefab);
        unit.HexUnit = unit.hexGameObject.GetComponent<HexUnit>();
        unit.HexUnit.Unit = unit;
        unit.SetGraphics(hexGrid.unitPrefab[(int)type]);

        float orientation = UnityEngine.Random.Range(0f, 360f);
        hexGrid.AddUnit(unit.HexUnit, location, orientation);
        unit.embark = false;

        playerUnits.Add(unit);
    }

    public void NetworkTakeDamageUnit(string data)
    {
        hexGrid = Object.FindObjectOfType<HexGrid>();

        string[] receivedData = data.Split('#');
        HexCell attackedCell = hexGrid.GetCell(new HexCoordinates(int.Parse(receivedData[0]), int.Parse(receivedData[1])));
        Unit unit = attackedCell.Unit.Unit;

        unit.Hp -= int.Parse(receivedData[2]);

        if(unit.Hp <= 0)
            RemoveUnit(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        hexGrid = Object.FindObjectOfType<HexGrid>();

        hexGrid.RemoveUnit(unit.HexUnit);
        playerUnits.Remove(unit);
        unit = null;
    }

    public void MoveUnit(Unit unit, HexCell end)
    {
        int xStart = unit.HexUnit.location.coordinates.X;
        int zStart = unit.HexUnit.location.coordinates.Z;
        int xEnd = end.coordinates.X;
        int zEnd = end.coordinates.Z;

        client.Send("CMOV|" + xStart + "#" + zStart + "#" + xEnd + "#" + zEnd);
    }

    public void NetworkMoveUnit(string data, bool isAI=false)
    {
        string[] receivedData = data.Split('#');

        int xStart = int.Parse(receivedData[0]);
        int zStart = int.Parse(receivedData[1]);

        int xEnd = int.Parse(receivedData[2]);
        int zEnd = int.Parse(receivedData[3]);

        HexCell cellStart = hexGrid.GetCell(new HexCoordinates(xStart, zStart));
        HexCell cellEnd = hexGrid.GetCell(new HexCoordinates(xEnd, zEnd));

        hexGrid.ClearPath();
        hexGrid.FindPath(cellStart, cellEnd, cellStart.Unit, isAI);

        cellStart.Unit.Travel(hexGrid.GetPath());
        hexGrid.ClearPath();
    }

    public Unit GetUnit(HexCell location)
    {
        for(int i = 0; i < playerUnits.Count; ++i)
            if(playerUnits[i].HexUnit.location == location)
                return playerUnits[i];
        return null;
    }

    public void AddCity(HexCell location, City.CitySize type)
    {
        client.Send("CUNI|CIC|" + (int)type + "#" + location.coordinates.X + "#" + location.coordinates.Z);
    }

    public void NetworkAddCity(string data)
    {
        string[] receivedData = data.Split('#');
        hexGrid = Object.FindObjectOfType<HexGrid>();

        City.CitySize size = (City.CitySize)int.Parse(receivedData[0]);
        HexCell location = hexGrid.GetCell(new HexCoordinates(int.Parse(receivedData[1]), int.Parse(receivedData[2])));
        location.FeatureIndex = 1;
        location.IncreaseVisibility();

        City city = new City(this, location);
        city.Size = size;
        location.Feature = city;

        playerCities.Add(city);
    }


    public void NetworkTakeDamageCity(string data)
    {
        hexGrid = Object.FindObjectOfType<HexGrid>();

        string[] receivedData = data.Split('#');
        HexCell attackedCell = hexGrid.GetCell(new HexCoordinates(int.Parse(receivedData[0]), int.Parse(receivedData[1])));
        City city = (City)attackedCell.Feature;

        city.Hp -= int.Parse(receivedData[2]);

        if(city.Hp <= 0)
            RemoveCity(city);
    }

    public void RemoveCity(City city)
    {
        hexGrid = Object.FindObjectOfType<HexGrid>();

        HexCell location = city.Location;
        location.FeatureIndex = 0;
        playerCities.Remove(city);
        location.Feature = null;
        city = null;
    }

    public City GetCity(HexCell location)
    {
        for(int i = 0; i < playerCities.Count; ++i)
            if(playerCities[i].Location == location)
                return playerCities[i];
        return null;
    }

    // TODO : faire la même chose que pour les city mais avec les ressources (add, take damage, remove, network, etc...)
    
    public void LevelUp(Unit.UnitType type) //To call this function using buttons, make them add as parameter the type of the unite in lowercase (cf Unit.StrToType for exact strings to send)
    {
        if (!Unit.CanAttack(type))
            return;

        if (unitLevel[(int)type - 2] < 20)
        {
            unitLevel[(int)type - 2]++;

            foreach (Unit u in playerUnits)
            {
                if (type == Unit.UnitType.REGULAR && u.Type == Unit.UnitType.REGULAR)
                    ((Regular)u).LevelUp();
                else if (type == Unit.UnitType.RANGED && u.Type == Unit.UnitType.RANGED)
                    ((Ranged)u).LevelUp();
                else if (type == Unit.UnitType.HEAVY && u.Type == Unit.UnitType.HEAVY)
                    ((Heavy)u).LevelUp();
            }
        }
    }

    /*
    public void IncreaseUnitLevel(int target)
    {
        while(unitLevel < target)
            LevelUp();
    }
    */

    public void SetDisplayer()
    {
        displayer = Object.FindObjectOfType<DisplayInformationManager>();
    }

    public void UpdateMoneyDisplay()
    {
        displayer.UpdateMoneyDisplay(money);
    }

    public void UpdateScienceDisplay()
    {
        displayer.UpdateScienceDisplay(science);
    }

    // TODO : mettre les bons displays (ajouter fer/or/diamant/nourriture/bois/chevaux

    public int[] UnitLevel
    {
        get { return unitLevel; }
    }

    public int GetUnitLevel(Unit.UnitType type)
    {
        switch(type)
        {
            case Unit.UnitType.HEAVY:
                return UnitLevel[2];

            case Unit.UnitType.RANGED:
                return UnitLevel[1];

            case Unit.UnitType.REGULAR:
                return UnitLevel[0];

            default:
                return 0;
        }
    }

    public void Newturn()
    {
        foreach (City c in playerCities)
            c.Update();

        foreach (Unit u in playerUnits)
            u.Update();

        ++roundNb;
        displayer.UpdateRoundDisplay(roundNb);
        displayer.UpdateMoneyDisplay(money);
        displayer.UpdateScienceDisplay(science);
    }
}
