using SimpleAimbot;
using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml;


// init swed
Swed swed = new Swed("cs2");

// a lot of adress will be relative to client.
IntPtr client = swed.GetModuleBase("client.dll");

// init ImGui and overlay 
Renderer renderer = new Renderer();
renderer.Start().Wait();

// entity handling
List<Entity> entities = new List<Entity>(); // all ents
Entity localPlayer = new Entity(); // only our character 

// conts 
const int HOTKEY = 0x06; //mouse 5 or 4
//under virtual key codes


// aimbot loop

while (true) //runs always
{
    // reset 
    entities.Clear();
    Console.Clear();

    //get entity list 
    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);

    // first entry
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    // update localplayers information
    localPlayer.pawnAddress =swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
    localPlayer.view = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vecViewOffset);

    // loop through entity list

    for (int i = 0; i < 64; i++) // 64 controllers.
    {
        if (listEntry == IntPtr.Zero) // just skip 
            continue;

        IntPtr currentController = swed.ReadPointer((IntPtr)listEntry, i * 0x78); // step = 0x78

        if (currentController == IntPtr.Zero) // same idea 
            continue;

        // get pawn 

        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);

        if (pawnHandle == 0) // obv
            continue;

        // second entry, and now we get specific pawn 

        // apply bitmask 0x7FFF and shift bits by 9.
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        // get pawn, with 1FF mask
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        if (currentPawn == localPlayer.pawnAddress) // if the entity is us.
            continue;

        //get scene node
        IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);


        //get bone matrix = get bone array

        IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80); // 0x80 is the value of the matrix 

        // get pawn attributes 

        int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);

        // if attributes holdup, we add to our own entities list 
        if (lifeState != 256) 
            continue;
        if (team == localPlayer.team && !renderer.aimOnTeam)
            continue;

        Entity entity = new Entity();

        entity.pawnAddress = currentPawn;
        entity.controllerAddress = currentController;
        entity.health = health;
        entity.lifeState = lifeState;
        entity.origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.view = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.distance = Vector3.Distance(entity.origin, localPlayer.origin);
        entity.head = swed.ReadVec(boneMatrix, 6 * 32); // 6 = bone id, 32 = step between bones coordinates 

        entities.Add(entity);

        // draw to console 

        Console.ForegroundColor = ConsoleColor.Green;

        if (team != localPlayer.team)
        {
            Console.ForegroundColor = ConsoleColor.Red; // if opps :)
        }

        // Console.WriteLine($"{entity.health}hp, distance: {(int)(entity.distance) / 100}m");
        
        Console.WriteLine($"{entity.health}hp, head coordinate: {entity.head}");

        Console.ResetColor();
    }

    //short entities and aim 

    entities = entities.OrderBy(o => o.distance).ToList(); // 

    if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) <0 && renderer.aimbot) // count, hotkey, checkbox
    {
        // get view pos
        Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view);
        Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);

        // get angles
        //Vector2 newAngles = Calculate.CalculateAngles(playerView, entityView);
        Vector2 newAngles = Calculate.CalculateAngles(playerView, entities[0].head);
        Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f); // set y before x.

        // force new angles
        swed.WriteVec(client,Offsets.dwViewAngles, newAnglesVec3);
    }

    Thread.Sleep(0); // adjust to your need. 

}

// hotkey import 

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);