BoardMeshModule is a simple 3d tileset module that can speed up grid-based world development. Tilesets can be defined by the user by following some naming conventions, and then painted into the editor via the BoardMeshComponent.

![image](https://github.com/meistermayo/BoardMeshModule/assets/22207902/911b8704-33e3-4c42-a871-41cf9b5fc622)

BoardTileSets expect to be pointed to a single fbx containing meshes with the following conditions:
- Required:
  - "floor"
  - "wall"
  - "ceiling"

- Optional:
  - "corner"
  - "inset"
  - "gate"

An example is included to show setup.

To install, just drop it into your Assets folder, or any preferred subfolder of Assets.
