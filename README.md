# ProceduralTextureGenerator

PTG is a simple, fast and easy to use tool for procedural creation inside of Unity 3D. It is part of my final project at university. 

## Video Demostration: https://www.youtube.com/watch?v=NYdrz8-DIh8
## PTG makes wide use of Compute shaders, so if your GPU doesn’t support them I’m afraid you won’t be able to use it.

## How To Use It
Once you import the package, you’ll notice that a new tab called “Tools” appeared (if not there already). From there you’ll be able to open the editor. 

![Image](http://i68.tinypic.com/2z4a0z5.png)

You’ll see the editor is just a blank grid, it may sound familiar if you’re used to work with node based workflow inside of Unity (animations, ...)

![Image](http://i65.tinypic.com/2uyhfsl.png)

To actually create textures from there you should creates nodes, and operate and do with them whatever you want. The workflow is similar to Substance Designer (not as powerful though ...). 

To create any node just right click inside the editor and a context menu will appear with all the options you have. 

![Image](http://i64.tinypic.com/2qstenp.png)

Generators are those nodes that not require from any input to generate the image, the Fractal noise for instance, that produces a fractal perlin noise. 

Operators are those that perform some sort of operation between pixels from the input.
Filters are those who given a texture modify its values through different algorithms, such us blur. 

Click one of them to create an instance of that node, normally you want to start with a generator. 

If you create a fractal node for example you’ll get the following.

![Image](http://i68.tinypic.com/2l8g0tj.png)

You can move the node along the canvas by clicking and dragging, right click hovering the node if you want to delete it. As you can see it only has an out point, since it is a generator node. 



If you click any node you’ll notice that its information appears in the inspector (as a regular Game Object). Each node has its own settings that you are able to tweak to your needs. 

All of them will give you the option to save its image as a .png. 
You can save any texture from any node at any point of the graph. 
So it doesn’t matter how many nodes you connect, you’ll be able to save nodes created previously. 

You can also change its resolution, doing so will change as well following nodes that the current node connects with. 

## IMPORTANT: In the case of nodes that has more than one input both input must have the same resolution, otherwise it would not be computed.

All the nodes generate seamless textures so you don’t have to worry about that, unless you make some sort of transformation to it using the Transform node (especially rotation will probably break the tilling). 

# Nodes
## Fractal Noise
It generates a multi octave seamless fractal perlin noise.

* Scale X: Determines the scale in the X dimension of the noise. Modifying it will stretch the noise horizontally. 
* Scale Y: Determines the scale in the Y dimension of the noise. Modifying it will stretch the noise vertically. 
* Start Band: Determines at which octaves it start the summation of noises.
* End Band: Determines the number of octaves (the higher the more accurate it will be). 
* Scale Value: It basically determines the highest number it can reach from 0 to 1, if set to 1 you will get full white colors.
* Persistence: Indicates the amount of difference between octaves. 
* Fractal Type: You can choose between: Fractal Brownian Motion, Billow or Ridged. 
* Random: It will randomly generate a new seed for the noise computation, you can also specify it yourself by typing the number. 

![Image](http://i64.tinypic.com/x45cmb.png)

## Cellular Noise

Cellular Node
It generates a seamless cellular noise (also known as worley or voronoi). 

* X Scale: Specify the scale of the noise in the X dimension. 
* Y Scale: Specify the scale of the noise in the Y dimensión. Normally scaling cellular noises doesn’t give good results, but you have the option. 
* Jitter: The amount of disorder between cells. 
* Random: Generates a new random seed for the noise computation. You can also type it yourself. 
* Cellular Type: You can choose between different types:F1, F2, DistanceSub, DistanceMult, One minus F

![Image](http://i63.tinypic.com/2e58ad3.png)

## Flat Color

Simply generates a plain textures with the chosen color applied. 

## Bricks 

It generates a seamless brick pattern. 

* Size: The number of brick per row and column. 
* Brick width: The width size of the bricks.
* Brick Height: The height size of the bricks.
* Mortar Width: The horizontal separation between bricks. 
* Mortar Height: The vertical separation between bricks.
* Brick offset: The offset between bricks from different rows.
* Brick rotation: Applies a rotation to all the bricks. 
* Brick Gradient: Generates a gradient at each individual brick.
* Gradient angle: Specifies the angle at which to place the gradient.
* Gradient strength: The amount of gradient. 

![Image](http://i66.tinypic.com/344pqo9.png)

## Parabola 

Uses a periodic parabolic equation to generate a texture.

* Count: The number of repetition inside the texture boundaries. 
* Width: The distance between picks.

![Image](http://i67.tinypic.com/2n6sshs.png) 

## Shape

It generates a procedural shape. You can choose between the following:

### Circle

It generates a procedural circle.
* Radius: Radius of the circle.
* Circle gradient: Fall Of of the circle. 

![Image](http://i67.tinypic.com/10wr31g.jpg)

### Square

It generates a procedural square. 
* Width: The width of the square (0 to 1).
* Height: Height of the square (0 to 1).
* Smooth edges: Smoothes the edges. 
* Square Gradient: Generates a gradient across the square. 
* Gradient angle: The angle that the gradient follows. 
* Gradient strength: The amount of gradient. 

![Image](http://i67.tinypic.com/juecnr.png)

### Pyramid

It generates a pyramid like shape. 

![Image](http://i66.tinypic.com/10h2jqr.png)

## Polygon 

It generates a x sided polygon. You can generate any shape with this node (even circles if you set the amount of vertices high).

* Vertices count: The number of sides of the polygon. 
* Falloff: Specifies the amount of falloff. 
* Size: Specifies the size of the polygon. 
* Rounded: Specifies if t has pointed or rounded vertices. 

![Image](http://i67.tinypic.com/2aang46.png)

# Operators

## Blend: 
It blends between two textures, you can specify which blending mode you want. (Just like in Photoshop). The blending modes are the following:


* Addition blend mode: As the name says it will add pixel values from both images, it will result into brighter images, a good practice is to add some sort of image leveling after blending. 
* Subtraction blend mode: It subtracts pixel values from both images, values below 0 are clamped to be 0 (black).
* Multiply blend mode: It multiplies pixel values from both images, allowing to transfer detail from one image to the other. It will darken the image.
* Add Sub blending mode: It is a combination of addition and subtraction. Foreground pixel values higher than 0.5 are added to their respective background, and foreground pixel values lower than 0.5 are subtracted instead. 
* Max (Lighten): It will pick the higher value between the foreground and background pixels. 
* Min (Darken): As opposite to Max blending mode, it will pick the lower value between foreground and background. 
* Divide: The opposite to multiply, it will divide the background pixels values with the corresponding foreground pixel. 
* Screen: Values from each layer are inverted, multiplied, and then inverted again. It gives the opposite effect to multiply, and will always give brighter results than the original images. 
* Overlay: It combines Multiply and Screen. If the value of the lower layer is below 0.5 then it will multiply, otherwise it will use the screen blend mode. 

## Mask Map

If you ever worked with the new High Definition Rendering Pipeline (HDRP) you’ll be familiar with Mask maps. 

It is basically a textures used to store values from 4 different grayscale textures in a single one. I called the node Mask map, to stay with the naming convention of Unity, but you can use it to blend any grayscale textures into one, each input node represents each color channel (RGBA), if you’ve worked with UE4 you’ll be also familiar with the MRAO textures, which essentially do the same as Mask maps, join several textures into a single one. 

In the Unity’s HDRP lit shader, a Mask map is expecting the Metallic texture in the red channel, the Ambient Occlusion texture in the green channel, the Detail map in the blue channel and the smoothness map in tha alpha channel. 

### IMPORTANT: When you save the texture it will be save as an SRGB texture, this is not what you want when working with this kind of textures, the same applies with other engines like UE4 with MRAO textures. After you save the textures, before using it make sure you uncheck the sRGB checkbox. 

# Filters

## Levels
It applies a level filters just as you would do any other image editing software, by specifying input and output levels you’ll be able to modify the contrast and brightness of the image. 

## Normal
Given a grayscale heightmap it generates a normal map. 

#### TIP: Blurring the image before converting to normal will give you better results.
### IMPORTANT: As with Mask maps, normal maps also need to be treated differently. Once you save the image, before using it make sure you change the texture type to Normal map. 

## One Minus
It inverts the grayscale colors of the image. As the name says it basically subtracts 1 minus the grayscale value of the texture. 

## Mix
Given two colors and a grayscale image it will linearly interpolate between those two colors based on the grayscale value of the texture. 

## Warp
It distorts the domain of a texture given another texture. (Useful for marble like textures). 

## Blur
Blurs a texture, you can specify the strength of the blur. 

## Transform
It allows you to tile the texture, move it vertically or horizontally and rotate it. Be careful since you can break the tilling. 




