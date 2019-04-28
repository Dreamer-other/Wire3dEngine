Wire 3d Engine

It's a render of multiple wire 3d objects with the correct view of their intersections.
It doesn't use techniques like Z-buffer but it processes the intersection of objects geometrically.

The solution consists of two projects:

1) Wire3dEngine is the core the main part of which is the processing of 
the intersection of objects and the preparation of visible data for displaying.

2) Demo is WPF app with some demostration of rendering. 
Result looks like this:

![alt text](https://user-images.githubusercontent.com/927867/56869249-d48eff00-6a06-11e9-94e7-2eafcc892603.png)
