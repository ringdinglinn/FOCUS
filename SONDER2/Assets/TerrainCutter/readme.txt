January 22, 2015

This was written for this forum post:

http://forum.unity3d.com/threads/terrain-cutout.291836/

As a curiousity I took a whack at carving and molding Unity3D terrain in real time, to conform to the bottom of buildings, and I got a preliminary version going.

It's complicated.

See the TerrainCutter in the enclosed unitypackage, and be CAREFUL. This will permanently modify your terrain heightmap data on disk, so make sure you are using source control in case there are any errors.

See notes in the script itself for more information. Basically the script traverses all renderers parented below the Terrain object, uses their MeshFilter mesh bounds box, scales and rotates that box to imitate the object's transforms (within important limits! Read the script), and then traverses the heightmap data and modifies any affected pieces of land that either need to be raised or lowered.

There are many spaces here: world space, local object, local terrain, heightmap array, and this script laboriously transfers and translates between them, introducing rounding and/or data loss errors at each step of the way.

Good luck. I'm done with this, if it helps anyone, that's awesome. :)

Kurt Dekker
