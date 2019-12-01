﻿using System;
using System.Collections.Generic;

namespace _20
{
    public class Square
    {
        public ContentType Content { get; set; }
    }

    public class Map
    {
        public int MinY { get; private set; }
        public int MaxY { get; private set; }
        public int MinX { get; private set; }
        public int MaxX { get; private set; }

        private Dictionary<(int y, int x), ContentType> _map = new Dictionary<(int y, int x), ContentType>();

        public ContentType this[(int y, int x) position]
        {
            get
            {
                if (_map.ContainsKey(position))
                {
                    return _map[position];
                }
                else
                {
                    return ContentType.Unknown;
                }
            }
            set
            {
                MaxX = Math.Max(MaxX, position.x);
                MinX = Math.Min(MinX, position.x);
                MaxY = Math.Max(MaxY, position.y);
                MinY = Math.Min(MinY, position.y);
                _map[position] = value;
            }
        }

        public void PrintMap(bool treatUnknownAsWalls = true)
        {
            for(var y = this.MinY; y <= this.MaxY; y++)
            {
                for(var x = this.MinX; x <= this.MaxX; x++)
                {
                    switch(this[(y: y, x: x)])
                    {
                        case ContentType.StartPosition:
                            Console.Write('X');
                            break;
                        case ContentType.Door:
                            var yTestValue = y == this.MaxY ? y - 1 : y + 1;
                            var charToWrite = this[(y: yTestValue, x: x)] == ContentType.Floor ? '-' : '|';
                            Console.Write(charToWrite);
                            break;
                        case ContentType.Floor:
                            Console.Write('.');
                            break;
                        case ContentType.Wall:
                            Console.Write('#');
                            break;
                        case ContentType.Unknown:
                            Console.Write(treatUnknownAsWalls ? "#" : "?");
                            break;
                    }
                }   
                System.Console.WriteLine();
            }
        }

        public int GetLargestNumberOfDoorsToARoom()
        {
            var gridXSize = MaxX - MinX + 1;
            var gridYSize = MaxY - MinY + 1;
            var grid = new RoyT.AStar.Grid(gridXSize, gridYSize);
            var startPosition = new RoyT.AStar.Position(0,0);
            for(var y = this.MinY; y <= this.MaxY; y++)
            {
                for(var x = this.MinX; x <= this.MaxX; x++)
                {
                    var content = this[(y: y, x: x)];
                    if (content == ContentType.Wall || content == ContentType.Unknown)
                    {
                        grid.BlockCell(new RoyT.AStar.Position(x - MinX, y - MinY));
                    }
                    else if (content == ContentType.StartPosition)
                    {
                        startPosition = new RoyT.AStar.Position(x - MinX, y - MinY);
                    }
                }   
            }
            var longestPathCost = 0;
            for(var y = this.MinY; y <= this.MaxY; y++)
            {
                for(var x = this.MinX; x <= this.MaxX; x++)
                {
                    var content = this[(y: y, x: x)];
                    if (content == ContentType.Floor)
                    {
                        var destinationRoom = new RoyT.AStar.Position(x - MinX, y - MinY);
                        var pathToDestination = grid.GetPath(startPosition, destinationRoom, RoyT.AStar.MovementPatterns.LateralOnly);
                        var lengthToRoom = pathToDestination.Length;
                        if (lengthToRoom > longestPathCost)
                        {
                            longestPathCost = lengthToRoom;
                        }
                    }
                }   
            }
            //Divide by 2 since we wanted to know number of doors passed.
            return longestPathCost / 2;
        }

        public int GetNumberOfRoomsWhichRequiresPassingAtLeast1000Doors()
        {
            var gridXSize = MaxX - MinX + 1;
            var gridYSize = MaxY - MinY + 1;
            var grid = new RoyT.AStar.Grid(gridXSize, gridYSize);
            var startPosition = new RoyT.AStar.Position(0,0);
            for(var y = this.MinY; y <= this.MaxY; y++)
            {
                for(var x = this.MinX; x <= this.MaxX; x++)
                {
                    var content = this[(y: y, x: x)];
                    if (content == ContentType.Wall || content == ContentType.Unknown)
                    {
                        grid.BlockCell(new RoyT.AStar.Position(x - MinX, y - MinY));
                    }
                    else if (content == ContentType.StartPosition)
                    {
                        startPosition = new RoyT.AStar.Position(x - MinX, y - MinY);
                    }
                }   
            }
            int numberOfRoomsFarAway = 0;
            int numberOfStepsToBeFarAway = 1000 * 2; //1000 for number of doors, multiplied by two for number of steps
            for(var y = this.MinY; y <= this.MaxY; y++)
            {
                for(var x = this.MinX; x <= this.MaxX; x++)
                {
                    var content = this[(y: y, x: x)];
                    if (content == ContentType.Floor)
                    {
                        var destinationRoom = new RoyT.AStar.Position(x - MinX, y - MinY);
                        var pathToDestination = grid.GetPath(startPosition, destinationRoom, RoyT.AStar.MovementPatterns.LateralOnly);
                        var lengthToRoom = pathToDestination.Length;
                        if (lengthToRoom >= numberOfStepsToBeFarAway)
                        {
                            numberOfRoomsFarAway++;
                        }
                    }
                }   
            }
            return numberOfRoomsFarAway;
        }
    }

    public enum ContentType
    {
        Unknown,
        Wall,
        Door,
        Floor,
        StartPosition
    }

    class Cartographer
    {
        private string _path;

        public Cartographer(string path)
        {
            if (path.StartsWith("^") && path.EndsWith("$"))
            {
                _path = path.Substring(1, path.Length - 2);
            }
            else
            {
                throw new Exception("Path did not begin with a start character or did not end with a finish character");
            }
        }

        public void Explore(Map map)
        {
            var stepIndex = 0;
            map[(y: 0, x: 0)] = ContentType.StartPosition;
            WalkUntilCurrentPathEnd(map, ref stepIndex, (y: 0, x: 0));
        }

        private void WalkUntilCurrentPathEnd(Map map, ref int currentStepIndex, (int y, int x) currentPathStartPosition)
        {
            var currentPosition = currentPathStartPosition;
            char currentStep;
            while(currentStepIndex < _path.Length && (currentStep = _path[currentStepIndex++]) != ')')
            {
                switch(currentStep)
                {
                    case '|':
                        currentPosition = currentPathStartPosition;
                        break;
                    case '(':
                        WalkUntilCurrentPathEnd(map, ref currentStepIndex, currentPosition);
                        break;
                    case 'E':
                        map[(y: currentPosition.y, x: currentPosition.x + 1)] = ContentType.Door;
                        map[(y: currentPosition.y + 1, x: currentPosition.x + 1)] = ContentType.Wall;
                        map[(y: currentPosition.y - 1, x: currentPosition.x + 1)] = ContentType.Wall;
                        currentPosition = (y: currentPosition.y, x: currentPosition.x + 2);
                        map[currentPosition] = ContentType.Floor;
                        break;
                    case 'W':
                        map[(y: currentPosition.y, x: currentPosition.x - 1)] = ContentType.Door;
                        map[(y: currentPosition.y + 1, x: currentPosition.x - 1)] = ContentType.Wall;
                        map[(y: currentPosition.y - 1, x: currentPosition.x - 1)] = ContentType.Wall;
                        currentPosition = (y: currentPosition.y, x: currentPosition.x - 2);
                        map[currentPosition] = ContentType.Floor;
                        break;
                    case 'N':
                        map[(y: currentPosition.y - 1, x: currentPosition.x)] = ContentType.Door;
                        map[(y: currentPosition.y - 1, x: currentPosition.x + 1)] = ContentType.Wall;
                        map[(y: currentPosition.y - 1, x: currentPosition.x - 1)] = ContentType.Wall;
                        currentPosition = (y: currentPosition.y - 2, x: currentPosition.x);
                        map[currentPosition] = ContentType.Floor;
                        break;
                    case 'S':
                        map[(y: currentPosition.y + 1, x: currentPosition.x)] = ContentType.Door;
                        map[(y: currentPosition.y + 1, x: currentPosition.x + 1)] = ContentType.Wall;
                        map[(y: currentPosition.y + 1, x: currentPosition.x - 1)] = ContentType.Wall;
                        currentPosition = (y: currentPosition.y + 2, x: currentPosition.x);
                        map[currentPosition] = ContentType.Floor;
                        break;
                    default:
                        throw new Exception($"Did not know how to handle character {currentStep}");
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Sample 1");
            Run(GetSample1Data());
            System.Console.WriteLine("Sample 2");
            Run(GetSample2Data());
            System.Console.WriteLine("Sample 3");
            Run(GetSample3Data());
            System.Console.WriteLine("Sample 4");
            Run(GetSample4Data());
            System.Console.WriteLine("Real Input");
            Run(GetRealInputData(), printMap: false);
        }

        static void Run(string path, bool printMap = true)
        {
            var map = new Map();
            var cartographer = new Cartographer(path);
            cartographer.Explore(map);
            if (printMap)
            {
                map.PrintMap();
            }
            Console.WriteLine($"It took at most {map.GetLargestNumberOfDoorsToARoom()} doors to get to a room");
            Console.WriteLine($"There are {map.GetNumberOfRoomsWhichRequiresPassingAtLeast1000Doors()} rooms which requires passing at least 1000 doors");
        }

        private static string GetSample1Data() => "^ENWWW(NEEE|SSE(EE|N))$";
        private static string GetSample2Data() => "^ENNWSWW(NEWS|)SSSEEN(WNSE|)EE(SWEN|)NNN$";
        private static string GetSample3Data() => "^ESSWWN(E|NNENN(EESS(WNSE|)SSS|WWWSSSSE(SW|NNNE)))$";
        private static string GetSample4Data() => "^WSSEESWWWNW(S|NENNEEEENN(ESSSSW(NWSW|SSEN)|WSWWN(E|WWS(E|SS))))$";
        private static string GetRealInputData() => "^NWWWNNENWNWWSSSE(SWWNWWWSSENESEESESWWNWSSSENEESWSSWNWSWSWSSSWWSSWNWNNWSSWNNNEEENNNNWNEENNE(SSSES(SWSW(SESWWS|NN(N|E))|ENNNW(NEWS|)S)|NWNNWWSWSEE(N|SWS(E|WWSSWNNNWNEE(SEWN|)NWWWNENEEE(SWWEEN|)ENWNNNEESS(WNSE|)ESENESES(EENNNWNEESENNEEENWWNEEEEEESENNNEESESSWW(N(N|E)|SEEESSEESSWSSWWSEEEENESESENNNNWNENNWWNENESENEENNWWS(WWNNE(S|ENENWNNNESSEESEEESWWWS(W(W|NN)|ESEEESESWSSSENNESSSENNEEEESESSWWSEESESSWNWNWWSWWWSSESESESEEENESESWWWWWSSENEEEESEEEENWNENNWNNEEENEENWWWNEENESESEENWNNNEENWNENWWWNNNEENEEESSSENNNEENENNNWNENWWNNNENNWSWSSWSSE(SESSESWWS(WNWWNWNWSWSES(WWWSWWSESESE(SWWSEEES(ENSW|)WSWWN(E|WSSS(WWWNWNEEE(SWEN|)NWWNNESEENN(E|W(N(E|WWS(WWWSW(SEENESSWWSESS(WNSE|)ESEESE(S(E|WWWN(E|W(SSS(SENNESSS(WSSENSWNNE|)E|WNWSWNNW(WW(SESE(SEWN|)N|W)|N))|N)))|N)|WNENNWSWWSESWWNW(WNNEE(SWEN|)NWWNWW(S(ESNW|)WW|NENWNNNNESESESSW(NWNSES|)SESEESSENENEENWWW(NEEENWNNWSSWNWNNE(S|EEEEEESWWWSESEESESES(EENNENNES(SSWENN|)ENNNNWSSWWWSWW(SESE(NENSWS|)S|NENNW(S|WWNENNENWWNWNNENNESSS(W|ENENWNNNESSENNNNEEEESWSWSSEEEENEESSENESSWSWWN(E|N(WSWNWSWSWNN(E|W(NNNNEWSSSS|)SW(N|SESWSW(N(N|WW)|SESEEENENN(WWS(WSEWNE|)E|NEESESSENNEENESEE(S(SSSSSWN(SENNNNSSSSWN|)|WWWWS(E|SSWSSSWNNNWW(SSE(SWSEEEESS(WNW|EN)|N)|W(NENE(NWN|SEE)|WWWW(WNWSNESE|)SSENES))))|NNW(S|NW(NEE(S|NNNWSSWNNNEENWWWWNNNNENESENE(SSWWSW(SEENES|N)|NWWNNWNNWWSWNWSWNNWNENWWWWNEENNEENN(WSWNWSSSWWNNWSSWSWNWNEENWN(WWWSE(SWSWSWSSWSWWSEESSENNNEEENWN(WSNE|)EE(SENESSENNE(NWES|)SSSWWWSESEESWSWNWWWSSWNNNNW(NEESE(NNEWSS|)S(E|W)|SSSWSESSWWSWSSESESWWNWNNNNWSSWWSEESSWWWSEESSSESWSWSESWWNNNWNNN(EESWSES(NWNENWESWSES|)|WWSSE(SSSSWNWNENWWSWWSWSEEE(NWES|)ESSWWSEEESSSSS(WNW(S|NNE(S|NWWWSWNWWWSWSESSWNWNNNENNWWWSWWSWNWNENNEEENNNEENNNWSSWNWSSWSESWWNWSWSESWSWWNWWNWNWSSSSEESSWSSENENESSWSESSWSESENENWNNNESEEESSS(WNNW(SSSWS(E|WWWNWSWWNWWSWNWWWSWSWNNENNNNESSSENNNNEEESWSW(N|SEEEN(EN(EES(E|WSS(ENSW|)(S|W(WWWW|N)))|NW(NWNEE(N(WNNNWSSWS(SSWNNNNNNNNWNEENNWWS(E|WNNWSSWSSSENE(NWES|)SSWWSESSENE(N(NN|W)|SSWSESWSWNNWN(WSSWSWWWWSEESSWWWSESESWSSESSWSWNN(NNNNWWWWNEENNNENWNWNWSSSWNNWWSESSEEE(SWWWSWNNWWSSWWWSWSSSWWNNNWSSSSESWSWWWNEENNWSWNWWNNWNWNNENESS(ES(ESS(WNSE|)EENWNENNNEENENWNNNWNENENEESWSEEESESWWWWN(EE|W(N|SSESSSWSWS(WNSE|)E(SSS|ENENNESE(SWEN|)NENWWN(WSNE|)EEE(SSEWNN|)NENWNEENENWNNENWWSSWWNWWS(WNWWWS(EE|SWNWNENEEEEEEEE(S|NENEENESS(WW|ESESSWW(SSESWWS(WSEWNE|)EEEEENNNEENNWNNWNENNWWWNENWNENEESWSEES(WW|SESWSEENEE(ENWNENENWNWSSWWNENNWWNNWSWS(WNWSWWNENWWWNENWNWWSESSWSWWNEN(E|WWNNNWWNENESEENNNNESENNWWWWNWNEENNEEESSWNWSS(EEESSESESSENESEESWWSWWS(WNWWNEE(EE|NNW(N|WWS(EE|SSWSSWNN(SSENNEWSSWNN|))))|SENESS(SEENNNN(WSSSNNNE|)EEESENNENNNNWSSSWSWNNW(SS|NWNEENWNWNENNN(EEESWWSSSENNESSS(EESSSSSSESENNNNENENNWSWNW(NNEES(EEENNNWWN(EEEEESSSWNW(NEWS|)SSSEEN(W|ESSENEESESWW(N|WSESENEESESENEENNENWWNNWNWWWWNENEN(ESEES(ES(SENNNN(W(WW|S)|EEESSESENNN(WSNE|)ESEEEEEESESWWWWN(WSWSW(NNEWSS|)SSEESSSEEENNNWNN(WS(SSSENSWNNN|)W(W|N)|ESESSEEENNESSSSSWWWW(NEEN(ESNW|)WW|SESWSSSSWWSEEENE(EENWWNN(WSSNNE|)NESES(W|ENNN(EE(NWWN(NNNNENES(SWEN|)ENNWWNWSWNNEEN(EEESE(N|SW(WWNEWSEE|)S)|WWWSSW(NWN(E|WWWWWW)|SES(WSSWNNW(ESSENNSSWNNW|)|EE)))|EE)|E(E|SWWSSE(NESEWNWS|)S))|W(S|WW)))|SSS(EE|WNWWSSE(ESWWWWNWWNEEE(NWNWNENEENWWWNNENN(ESE(SSW(W|N)|N)|WWS(SSSS(SSS(WSSWWWNNESENNENNWWS(SWWNNE(S|NNNWWWNN(WSW(N|SESESE(NESNWS|)SSSSWNWWNNE(ESWENW|)NWNWSSSWNWNNNN(WSSSSWNNWSSWWNNE(S|NNN(WN(NWN(E|NWWWSW(NNEWSS|)SSSEEESE(S(ENSW|)SSWNWN(NWWWW(NEN(N|W)|SSWWSEEENESSEE(NWNNWW|ESSWSWWWSWW(SES(WW|EEEESW(SSESEES(ESEES(W|E(NNNE(ENNNENWWWNNESEENEN(ESSEN(ESS(EE(NN(WSNE|)N|SSEESWWSSENESE(SSSEE(EE|SWWS(EE|WNNNNWSS(SS|WWNENWNWSSSWWSWN(SENEENSWWSWN|))))|ENEENNESENNWWWWSW(S(EENSWW|)S|NWWNEEEEE(WWWWWSNEEEEE|))))|WSWW(SWS(S|E)|N(E|N)))|N)|WW(WNN(ESEEWWNW|)WSSWSWWSEESSSS(EENNWS|SW(SEWN|)NW(WNW(W|NNE(EESSWNW(ESENNWESSWNW|)|NWN(EN(W|NN(ESSENNES(NWSSWNSENNES|)|N))|WWS(E|W(W|N))))|S)|S))|S))|S)|S))|WWW(N|SESWW(SS|NN)))|WWW))|NNW(NEES(EN(ESENEWSWNW|)NWWWWSSWNWNW(S|NWWNNESEESE(ENWN(E|N)|S))|S)|S))))|E)|NNWWN(ENW|WS)))|E)|EE(SWSEWNEN|)E(E|N)))|ES(ENSW|)SS))|NESENNNNW(SSWENN|)N(WW|EEEN(WWWNNNESSE(WNNWSSNNESSE|)|ESSWWSSENESEE(NWES|)SWWSSW(SSEE(SWWEEN|)N(W|NN)|W(NNESNWSS|)WW)))))|E)|E)|E)|E))|S)|N)))))|EE))|W)|WWW)|WWWS(E|WNWSSSEE(NWES|)S(W|EEES(W|E(ESWS(SENES|WN)|N)))))))|WWWSS(WNW(NE|SSSEN)|EE(ESEN|NW)))|W)|SS(SSS|E))|WSS(W|S))|WSSWWNN(ESNW|)WWWW(WWWWWWWSES(ENSW|)WWWWWSESSESSWSSWNNN(NNWSSWSWWNNNES(ENNNE(S|NN(ESENESEN(SWNWSWENESEN|)|WSWWWSS(EENWESWW|)WSWNWWNWSWSSSWWSW(NNNNESE(SWEN|)NNNE(S|EN(WWWWSESW(ENWNEEWWSESW|)|ESEEE(SWWEEN|)N(WW|EEE)))|SSSENNEEN(ENNEE(NWWEES|)SWSEESWWSESWWN(N|WSSESWWN(WWSESSSENNESEESWWSWSSWNNW(SSSSEEEEEENNENENNWW(NNESENNNNENW(NNNE(SSEESSW(SSENENE(NN(WSNE|)N|SSWSSSESEESWWWSEEEENENEEE(SWWSEESWSSWNWN(E|WSWSEESWSWSEENEEEENEEE(NESEE(NWNWWNWNEN(EN(W|ESSS(WNSE|)E(NEWS|)S)|WWWSSE(N|SS(ENSW|)WNWSSWW(EENNESNWSSWW|)))|SSESE(NN(E|W)|SESS(E|W(N|SW(SESWENWN|)W))))|SWWSWWS(E|WNWSWWWWWNNE(N(E|WNNN(WWNNN(E|WW(NNN(ESE(SWEN|)N|NN)|SESSWW(NEWS|)SSW(NN|SESESESENNWNWNNEEESW(SESSSS(E|WSESWWSSWWNENWNEENWWWWWWSSSSESSSEENN(NW(NENWW(NEEWWS|)S|SS)|EESSESWSS(EESS(WNSE|)SENNNEENNNWSSWW(NENNW(N(WSNE|)NNEES(WSESNWNE|)E|S)|W)|WNN(WSSSWWNWWWNNNNESE(NNWNWWNW(SSSSE(NNEWSS|)SWSESWSESSSW(NN|SSENEENNEN(NWSW(N|SS)|ESSWSSSESSENENE(SSWSWWSEEENEE(NWNEWSES|)SESSSSWWSESWWSSEESENENNESSEEESWWWWSSEEN(W|EESSSEEENWNW(NNNNWWNNW(NWW(NNEES(ESESEENESEENNW(S|NENENWWNWWNWN(EEENWNENESSSS(WW|SENEEEESWSEESWWWSSSSWNW(SSSEEEN(EEEEEE(NWNWSWNNENNEN(WW(SSWWSESWW(SEEWWN|)NN|NWNENNNWWWSWNNWW(SSSE(NN|EEEENW)|N(WWWSWSS(NNENEEWWSWSS|)|EEE(SEEEESSSSS(NNNNNWESSSSS|)|N)|NN)))|ES(SWSNEN|)EENN(WSNE|)E(NWNENWN(W|EEE(SWSEENEESEE(SS(SESNWN|)WWWN(EE|N)|NN(NWS(W|S)|ESENE(S(EN|SWW)|N)))|NWNENNE(NWN(NE(S|N)|WSW(SS(ENSW|)SW(SE|NW)|N))|S)))|SS))|SSWWWWW(NEEEEWWWWS|)SESWWNWN(E|WWWWWSW(SEES(ENN(W|EESESEESESWWNWWNW(SWWSSWWSSSEEEESENNESSSESENNWNENNN(WS(S|WWNENWWSSSWN(WWSEWNEE|)NN)|ESEESWSESEENESESESWWWSESSWNWNNN(EE|WWSW(NN(E|NN)|SESWWWSWSWWWWSSWWNWNWNNNEENENWWSWNWNEE(EEESE(N|ESSES(EN(NWNSES|)EE|WS(WNNW(NEWS|)WSWWSEE(ENSW|)S(S|W)|E)))|NWWWNWWNWWSESEESSE(SSSWSSESWSSWWWWWSSSENESSWSWSWSEESWWWNWSWSSSWWWNNW(NEEE(SWSEWNEN|)NNE(S|NNESSENNNNNNNE(SSSSS(E|S)|EENNEESWSEENNNNNWSWNWWNENEN(EESWS(EESNWW|)W|NWWS(SWSWWNWNWWWNNNW(SSSSSSESSSSENEENEENN(WSWNNWNWS(WNSE|)SSSENN|ESEES(WWSS(ENSW|)WW(NEWS|)SWNWSWSWSW(SEESSE(SWSSWWNENNNWSS(NNESSSNNNWSS|)|NEENWNENWWS(W|S))|NN(E|NNNN))|EE))|NEESEENNEENWNNWWWNEEENNWWS(WWSW(SESWSSENEE(ENWWEESW|)S(W|S)|NNNESENENESEEE(SSSSENESSWSESENE(NWNEWSES|)ESES(WW(WWSWS(E|WNWSW(S(SE(NESNWS|)S|WWWNEE)|NNEEN(ESNW|)NNWNNNN(SSSSESNWNNNN|)))|N)|SENE(EESWWSEE(WWNEENSWWSEE|)|NWN(ENSW|)W))|NNWSWNWWNEEENN(EEE(ESSWNWWSESS(NNWNEEWWSESS|)|N)|WWNNWWNWSW(NNEEE(ESWENW|)NNWSWW|SSENESE(N|S(WSSWW(NENW|SEE)|EE))))))|E))|E))))|SSSEEESSEESWWSSENEENNESESESSSEENNW(NNWNWNWNEENWWNWSW(SWSEEN|NNEEESEESESWS(W|ES(ESWSESSSEEEEESSSSWNNNWWWSWWWWN(WSSSWWSES(WWNWNWWSWW(NNE(NENENN(WWS(W(NNNEE(SWEN|)NWNW(NEESNWWS|)S|S(E|S))|E)|ESSENESE(NENWN(WWSEWNEE|)(N|E)|SWWS(WNWSWENESE|)(EE|S)))|S)|SEEE(E|N))|EEN(EEEENEE(SS(EEN(W|EEENNEENNWNENENESESWWSEEESENESESSWSWS(EEEN(W|NNNESEEESEESS(EENENWW(NWWNEEESEENESESWSES(WW(W|NN)|EENEENNWNWW(WNNESEENNW(S|WWWSWS(W(NWNENE(S|NWNNNENENNENEESENNWWWNWWNEENEESWSEEESSEENNNNNNWWNEEESSSSSENEENNENENWNNWSSSWNW(SSESWENWNN|)NNE(S|NENWNENEEESWSW(N|SEEESENNW(W|NENWNEEENNEEEEENNEENNNNWSWWNNE(S|EENNEESWSSESSSSSEENNEESWSEENENNW(WWNENESENEN(EEESSWNWSSSW(NN|SSESEESESSEEEENNWSWWNNE(NESENNWWNWSSW(WWN(NNEE(SSWNSENN|)NNEEN(WWN(E|W)|ESENESESWSS(WNNWWWSEES(NWWNEEWWSEES|)|SEEENNNNENWNENWW(NNNN(W|EESWSEEEESESESSSWNWWW(SEESWSSENESEENEESWSSENENNNNNENNWNNNNWSWWWNWS(WWNENN(EES(EE(SWEN|)ENNWSWNNENESESEE(SWWSEESWSSSE(SSSSWSSE(N|SWSSWSSWWN(WSWNWWNN(ESEENW|WSSWNW(NENWNNES(NWSSESNWNNES|)|WWSWW(NEWS|)SESSSENNEN(W|E(NWES|)SENESENESESWWSEEEENN(WSNE|)ESENEE(NN(WSNE|)NN|SSSWWSESE(SSSWNWWSESSWWN(NWWWSSWSESENNNESSSSESSSWWWWSEESENEENESSESE(NNNW(S|NNNE(NNNW(NEWS|)SSWSSW(SEWN|)NNWNEE|SS))|SSWWSWSWNNWSSWNWWWNWWWSEESEEESWSWWSSESSWWS(WNNNWNWWNEEENE(EE|NWWWS(WNWWWSSWSESWSWSEENESENN(ESSS(WWWWWWNNWWSWS(EENSWW|)WNNWWNWNNENNNESESSENENEENNEENEENWWWNWWNWWWWNWSWSSESWS(WNNWSWNWWWWWSWNNWWSESSSENEENESSESSWSWNWWN(EE(NWES|)E|WWWWNNWNENWNNNENESES(ENNESSEEES(EENWNENENEESWSSW(N|SEENNENENENESEEESES(ENEESWS(SEEN(NENENNNWNNNNENNWSWWSESSSWWWSSEEN(W|ESS(ENSW|)(WWWWNWWNNESENNWNNWNWNWSWWWNNWNENESSSENENESENEESWS(EENEEE(NWWNN(ESEEN(E(SSWSEESSSSSS(WNNSSE|)SSESSEEESE(NNESEEENWWN(E|WWN(E|WWW(NNNW(NNNESSENNESESE(NNNW(S|NN(ESESESS(WNSE|)ENE(EE(E|NWNN(NEENSWWS|)WSSWNNW(W|S))|S)|NNWSSWSWN(NNESNWSS|)WS(WN|SEEEN)))|SWW(N|WSSENES(NWSWNNSSENES|)))|SSS)|SSENES)))|SSSS(ENNESES(ENENESS(E(E|N)|S|W)|WS(W|E))|WNNNWNWWSSWWSESE(EENNW(SWEN|)N|SWS(E|WNW(N(NNNENE(S|NN)|E)|WS(E|W(N|WSW(S(E|SSWWS(EE|WN(NENESNWSWS|)WWS(ESNW|)WN(NN|W)))|N))))))))|NN)|W)|WWWWWWW(WNNWWNN(EES(ENE(ESSENESS(ENSW|)WWWN(WSNE|)N|N)|W)|WSWNWN(WWSSE(N|ESESSEE(ENWWNSEESW|)SWWWNNWNWWWSSEE(NWES|)SWWWNWNW(WWSSSSESEEEESENNN(ESEN(NN|ESESSEE(NWNNWESSES|)SSEEEN(WW|EESWSE(ENSW|)SWS(ESNW|)WNNWWWSWWNENWWSWWWSSEEN(W|ESEES(WWWWSWSE(ENEWSW|)SWWNNWWW(S|WNNESEEE(E|NWWNWWWWSWNWWW(SSWNWSSSEEEN(NE(NWES|)SSSE(NENN(WSNE|)E(S|N)|SWWSSWWWWNNEN(WNNNWSSSSSSSWSWSSSW(SEESENNWNNEEEESEEESWWWWNW(SSSSEENWNEEEEEEESSSESS(WWN(WWWNENN(EESWSE|WSW(SWWWSWS(WNW(SWWEEN|)NEENWWNW(ESEESWENWWNW|)|EEEE(NWWEES|)EEE)|N))|E)|ENNEEENN(EEEEN(NNEEN(ESE(N|SWWSSE(SSSS(EEN(ES|WN)|WWWNNEN(ESSWENNW|)(WWSSWNW(NEWS|)SWWWSEEEEE(WWWWWNSEEEEE|)|NNN))|N))|WN(W(WNEWSE|)S|E))|W)|NWWSS(ENSW|)WWNENWN(EE|WWNW(S|N(NWSWNN(WWS(ESNW|)WW(NEWS|)W|ENE(NWES|)S)|E)))))|W)|NNWNNE(S|NWNWNEN(WW(N|WSESWSEESSWNWW(WNENSWSE|)SSE(N|EE))|ENEESSSSW(NWNENSWSES|)S)))|EE(E|SWS(W|E))))|WW)|NENENNN(NWSNES|)ESENESEESSES(WWWNENWWSSW(W|NN)|ENN(WNEWSE|)ES(SSWENN|)EEENNE(SEEWWN|)NWWSSW))))|ENEE(NW|SWS)))))|WWWS(WNN(W(NEWS|)S|E)|EE))|NEN(E(EE|SS)|W)))|NNNN))|S(EEEEEES(NWWWWWEEEEES|)|S)))|SWWSWSS(WNNWESSE|)E(N|S(ENSW|)W))|W)|S))|W)|WW)|WWWN(W(S|W)|E)))|WW)|WWSESSSSENN(SSWNNNSSSENN|)))|EENNEESSW(N|SE(S|EENWNEE(NWWNWWWW(EEEESEWNWWWW|)|E))))|ENN(ESSNNW|)N)|WNNE(NWES|)S)|E)|SS(SSEWNN|)W)|EEEEEENNWSWNW(NN(EES(W|ENESESSS(WNNSSE|)ENEE(SWEN|)NWNENWW(NN(NENESSWSE(WNENNWESSWSE|)|WWWWS(EEESNWWW|)W)|SS))|W)|S)))|E)|N)))))|E))|NN)|NNW(S|NW(S|NNWWW(NN(ESENESENE(NNWN(EN|WSSE)|SSS(WNSE|)S)|NWN(ENW|WSWN))|SES(ENSW|)WWS(WNSE|)(E|SS)))))|W)|WSW(SS|W))|SEEESW(SSESSS(W(W(S|W)|NN)|ENNN(E|N(NN|W)))|W))|NNN(ESES(E|W)|WW)))|SS(SSSWSE|W))))|W)|S)|S))|WNNWSSWNNWWSSE(SWSW(NN(NNNWNENESES(W|ENNE(NWNNWNENNWSWSSWNNNENWWSWNWSSSWW(SEESSWNWSSSWNWNN(ESNW|)WNWWSWSEE(N|SSSSSSSSEE(NNNNN(NW(NN|SSSSS)|ESENE(SSWWEENN|)NESENN(ENWNNNE(NWES|)SSEES(E(S|N)|W)|WW))|SSWSEE(NEN(ESNW|)W|SWWWWNENWWNE(NNNWSWNWNWNNNWWNNENNWSWSWNW(WWWN(W(W|SSSESSESSEEENESSWSEENE(SE(N|ESWSESWSSSS(ENNNE(NNNEWSSS|)SE(SWSEWNEN|)N|WNWSS(WNNNWSWWNWWSESESES(ENNWESSW|)SWNWWN(WSSSEE(NWES|)SWWWSWWNNWWNNWSSSWSEENESSESSEE(S(WWWNWSWWNNN(WWSESWSSSESWWWNENWNNNWWSWSWNNENNEEENWNEEENNNNWW(NENEN(WW(S|N(E|W(WNNWSWW(NENNWNENESE(SSWNSENN|)NN(WWN(ENNSSW|)WSWW(NEWS|)S(E(E|SSS(WNNSSE|)E)|W)|ESSEENNW(NN|S))|S)|S)))|ESENEESWSSEEN(W|ENWNNNESES(SESWSWSEE(NE(E|NNNWNNWNEENWNN(WSWNWSWSES(ENEWSW|)WSSWWNE(WSEENNSSWWNE|)|ESE(NNWWNEN(SWSEESNWWNEN|)|SESSW(WSEES(W|ENNNE(SSS(S|EENNW(S|NEEESSSW(SEWN|)NN))|NNW(S|W)))|N))))|SWS(WWN(E|NWWWSW(NNENWW|SS(ENSW|)SWS(W(N|S(WWW|SSS))|E)))|S(ENSW|)S))|W)))|WWSEESE(SWWW(NEWS|)SWS(E|WNN(WNW(SWWN(WSWSSWSSSSWSESENENWNNNESENNEEN(WWWSNEEE|)ESSS(ENSW|)WW(NEWS|)SW(SSENESSWWWSSSESWSSSSEESSSWSESEESES(WWNWWNW(NWNWWNEEE(ENWWWWNWSSWNWNWSSESSWWS(WWWWNNNW(SSS|NNWNNNESEENEEENESESS(E(S|NNESEENWNNENE(NWWNWSWSE(E|SSWWN(NWNWWWSEES(E|WWS(E|WW(S|WWNNNWN(NNESSESSES(W|ENNWNNESENNWNENWW(SSW(S|NNWSWNWW(EESENEWSWNWW|))|NEN(N(ESSE(EEEESWWWSESESE(NE(S|N(ENNNN(WWSESS|E)|WW))|SW(S|WNWNW(SSEWNN|)NN))|N)|NN(W|NNN))|W)))|WS(W|SSS(E(S|NN)|W))))))|E))|SS(W|SSSSWNWW(EESENNSSWNWW|))))|WWSSSWNNNN(EE|WSSWW(N(W|E)|SESSSEEN(WNNSSE|)EE))))|SESS(E(E|NNN(ENENWESWSW|)W)|W(SESWENWN|)N))|S)|S)|ESEENNESENE(SEWN|)NNENNNWWNWWNENWWSWSESSSENE(NWES|)EESWWSWS(EENSWW|)WS(WNN(E|NNWN(WSSES(WWNNNNENWWWNEEEE(ENEENWWWWNEEENNN(E(N(N|W)|SSSESES(ENN(W|EE(ES(ENSW|)SSWNNWSSS(SWNNWS|E)|NNN(WNSE|)EE))|W))|WWW(NNNWESSS|)SWWWSESE(SWS(W|EEE)|NEENES))|SS)|S)|E))|S))|W)|E)|NNN)|E))|N))|EES(WSNE|)E)|SES(E(NNWESS|)SS|W))|NWNEENEES(ENNENW|W))|E)|EE)))|NNW(S|N(E|WNWSWW(SEWN|)NEN(EE|NWW(W|S(S|E)))))))|E)|NEENNNWSW(SEWN|)NWNW(SS(E|W)|NNN(EESWSEENNN(WWNEEWWSEE|)ESE(N|SWSS(ESENN(W|EE(NWWNEE|E(SSWW(NEWS|)SWS(WNSE|)ESESWSS(W|EE(SSS(WNNSSE|)SEENNNNWSSS(NNNESSNNWSSS|)|N(NNEWSS|)W))|E)))|WW))|WS(WNSE|)S)))|EE))))|NNE(NNW(S|NEN(W|NESSSENNNNN(N|ESSSSSEE(NNW(NENNWS|S)|ESE(ESES(ENENNW(NEESE|SW)|W)|N)))))|S))|SE(N|ES(WW|SS))))|W)|S(SS|E))|N))|S))))))|S)|E))|SS(E(N|E)|S)))|S)|WNWWWS(WNNEEWWSSE|)EE))|WNNWN(EE(E|S)|WSSS(ENSW|)WWNW(SWWWEEEN|)NEE(NNEEWWSS|)S)))|W(WWWW|N))|NNWSWWSWWN(WSNE|)E)|W))|EEE)|W)))|S))|N)))))|N))|W)|NNN(ESEENW|WW))))|WW)|NEN(NNENWESWSS|)W))|WSSESESSE(N(E|N)|SWW(WNWNN(ESESNWNW|)(N|WWWSEE)|S))))|W)|S(WSSWWEENNE|)E)|SS)|S))|NWNW(NNN|S(WNSE|)S))))|NNNNEENES(SSSW(SEWN|)NW(NEWS|)S|EENEE(ES(WW|E)|NWWNN(ESNW|)WWWWWSSSENNEES(W|E(S|N)))))|SWSEENESS(NNWSWWEENESS|))|NN))))|W))))|E(SS|E)))|S)))|N)|NWNWNENWWSSWS(EE|S|WNW(S|N(WSNE|)EENWNW(S|NEEE(SWEN|)NENEENN(ESSSNNNW|)WWN(E|W(SS(SWSNEN|)EE|NN(ES|WS))))))))|N)|N(WWSNEE|)N)|WSSWWS(SWEN|)EE)|S(SWW(NEWS|)SESWW(NN|WW)|E))|NNN)|NN))|W))))|S)|E)|SSES(W|ENN(ESSESSSS(WNNNWWSES(NWNEESNWWSES|)|E(SEWN|)NN(NN|E))|W)))))|WW))|W))|ESE(S(SSESEN|W)|E))|SSW(N|SESSSWSWW(WWSSSS(W(SSS(WSSNNE|)EE|NW(W|S))|ENEE(SWEN|)NWWNEEE(ENESNWSW|)SS)|NNNNN(WSNE|)ESSESWS(NENWNNSSESWS|)))))|W|N(E|N)))))|ESES(WWN|ENES|S)))))|W)|W)|NN)|E)|E)))|E)|E)|S)|S))|W)))|W)|ENESENEESWSW(SSEEN(NEE(SWSESWWSS(WNNWESSE|)E(SSSE(NNEWSS|)S(ES|WWS)|N)|NNW(N(WWNWSWNNENWWS(SS|WWNEN(WWS(S|WWNEN(NWWS(WNNWWN|E)|E))|ENESENNE(WSSWNWESENNE|)))|EENE(SSWSEWNENN|)NN(EENSWW|)WNWSS(E|S))|S))|W)|W))))|ES(ENNNW(S|NNNNEEN(W(WWSNEE|)NNN|EEEEESSS(WWW(SEES(SSESSWN(SENNWNSESSWN|)|W)|NN(ESENSWNW|)WSSWNWSS(NNESENSWNWSS|))|EEENWNNENE(NWWWWNW(SW(N|SEEE(SSS|E))|NNEES(W|SEEENWNNNN(WWSES(WWNNW(WSESWENWNE|)N|SS)|ENENEES(ENNNWNENWNN(ESEESESEENWNWNEEEENWWNEEN(WWWW(SS(WSW|EN)|N(ENNW(NEESSSEN(SWNNNWESSSEN|)|S)|WW))|EEESE(NN|ESWWWNWSSEE(E|SSSSWWNNE(NWWSSSS(ESESWW(N|S(EEENN(NWES|)ESSSENNN(SSSWNNSSENNN|)|WS(W|E)))|WN(NNNWESSS|)WSWWN(WSW(SS|NN(N|E))|E))|S))))|NWSSWWSSSWNWS(W|SES(ENENE(SE|NNWS)|S)))|SSWW(NENSWS|)SW(SEWN|)N))))|ESESWW(SSENSWNN|)(N|W)))))|W))|N)))|NW(W|N))|E)|EEEESS)|ESSSEENNW(S|NEESSESEENE(NNWSW(NWSNES|)S|SSWSWSESW(SSESSENNN(NNN|W)|WWWNNEN(E(SSWENN|)E|N|WWW(WWNEWSEE|)SSE(SWEN|)N)))))))|S)))))))|N))))|WW)|WWNWNWSSSE(S(WS(WNNWESSE|)E|ENESEN)|N)))|S))|S))|E))|S))|ENEEE(N|S)))|NN(NEEWWS|)W)|E(N|EE))|E)|N)))|E))|WW(WWNSEE|)SS))))|NN)$";
    }
}
