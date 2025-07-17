#import "@preview/rubber-article:0.4.1": *

#show: article.with(
  header-display: true,
  header-title: "Realistic Water Shader and Optimizations",
  eq-numbering: "(1.1)",
  eq-chapterwise: true,
  margins: 0.5in,
  lang: "en"
)
#show link: underline
#show link: set text(fill: blue)

#set image(width: 70%)
#set text(
  font: ("Libertinus Serif", "芫荽"),
  size: 11pt
)

#let sideBySideImages = (img1, img2, caption: "") => {
  figure(
    stack(
      dir: ltr, // left-to-right
      spacing: 2mm, // space between contents
      image(img1, width: auto, height: 16%),
      image(img2, width: auto, height: 16%)
    ),
    caption: caption
  )
}

#maketitle(
  title: "Realistic Water Shader and Optimizations",
  authors: ("B11902109 侯欣緯, B11902140 洪銘德, B12902119 胡祐誠",),
  date: datetime.today().display("[day]. [month repr:long] [year]"),
)

#align(center)[
  #set par(justify: false)
  #set text(size: 14pt)
  *Abstract*
]
In this project, we use Unity with the High Definition Render Pipeline (HDRP) to simulate realistic and dynamic water surfaces.
Our implementation combines multiple Gerstner waves to replicate natural wave motion, and layered noise textures to introduce fine-grained surface variation.

We leverage scene depth and screen-space techniques to dynamically adjust water transparency, color, and blur based on the viewer's perspective and water depth.
Chromatic dispersion effects are introduced for added visual realism, simulating light separation near submerged edges.
Additionally, foam is rendered along shallow regions using a depth-based intensity gradient, simulating the interaction between water and solid objects.

Our water system is demonstrated across two different scenes, an open seascape and a swimming pool, to showcase adaptability and realism in varying environments.

= Introduction
Water rendering is one of the most visually demanding elements in real-time graphics, requiring the simulation of complex physical behaviors and interactions with the environment. 
In this project, we aim to create a visually compelling and physically plausible water surface using Unity's High Definition Render Pipeline (HDRP).

Our goal is to reproduce the essential characteristics of real water:
undulating surface motion, light refraction and scattering, depth-dependent opacity, and shoreline interactions.

The GitHub link of our project: https://github.com/HyperSoWeak/113-2-ICG-Final-Project

= Implementation Details
We begin with an $(100 times 100)$ plane mesh composed of 200 isosceles right triangles.
// #figure(
//   image("plane mesh.png", width: 40%)
// )
To enhance the visual experience, we incorporate various 3D assets, such as rocks, an island, and a boat, from Unity HDRP sample assets to build the first scene.
Additionally, we imported an exterior swimming pool scene from the Unity Asset Store.

With these two environments, we demonstrate the adaptability and realism of our water surface under different contexts. The final setup is shown below:

#sideBySideImages(
  "scene_pool.png",
  "scene_beach.png",
  caption: [The scene built to enhance visual effects.],
)

== Vertex Animation
We simulate wave motion by combining three Gerstner waves, each with different velocities, wavelengths, and amplitudes, to update the vertex positions and normals.

Each Gerstner wave uses a cosine function to model the vertical displacement of the water surface.
To enhance realism, we also incorporate a sine component that offsets the cosine wave slightly, increasing the variation and exaggerating the wave peaks for a more dynamic appearance.
#figure(
  image("vertex animation.png"),
  caption: [Water surface using a combination of three Gerstner waves.]
)

== Surface Ripples
To create a more uneven and realistic surface, we apply three normal maps with different movement speeds and intensities.
These maps perturb the fragment normals and make the surface more uneven.
#figure(
  image("surface normal.png"),
  caption: [Enhanced surface detail using layered normal maps.]
)

== Transparency
Instead of directly adjusting the alpha (opacity) value of fragments,
we achieve transparency by sampling the scene color from the current camera's color buffer.
This approach allows for more realistic water rendering while maintaining fine control over clarity, displacement, and color.

=== Integration with Surface Ripples
To enhance realism, we displace the sampling position based on the fragment's surface normal.
This simulates how surface ripples distort the view of objects beneath the water, making the transparency appear more dynamic and physically accurate.

=== Chromatic Dispersion
Leveraging our scene color sampling method, we simulate chromatic dispersion,
a visual phenomenon where light splits into different colors due to varying refractive indices.
We offset the sampling position slightly for each color channel, i.e., red, green, blue,
which creates a subtle rainbow-like rim around submerged objects.
// particularly noticeable at the edges of the water surface.

=== Dispersion
As we achieve transparency by sampling the scene color, we are able to simulate dispersion effect caused by moving water
by slightly moving sample position of different color channel.
Result in the rim of objects in water has a "rainbow" rim.
#figure(
  image("chromatic_aberration.png"),
  caption: [Transparency result with dispersion. \
  (Note: The dispersion effect is exaggerated in this image for clearer visualization.)]
)


== Depth-Based Effects
Using Scene Depth and Screen Position, we calculate the distance from the camera to the objects beneath the water along the viewing ray.
We then use this depth information to non-linearly adjust the surface's opacity and the chromatic dispersion and blur intensity.

This depth-based modulation mimics real-world water behavior:
- When viewed from directly above, underwater objects are more visible.
- At shallow viewing angles (i.e., more parallel to the water surface), the water appears more opaque.
#sideBySideImages(
  "depth_parallel.png",
  "depth_above.png",
  caption: [Depth-based opacity and effects with comparison of different viewing angles.]
)

== Foam
To make the edges of the water surface appear more natural, we add dynamic foam in areas where the surface depth is very shallow.
The foam is applied with 3 different intensity according to the depth, preventing harsh transitions between foamy and clear regions.
This creates a more organic and visually pleasing effect at the water-object boundaries.

#figure(
  image("foam.png"),
  caption: [Foam effect at the water's edge]
)

== Reflection

To simulate realistic reflections on the water surface, we combined baked reflection probes and Screen Space Reflection (SSR) within Unity HDRP.

Baked reflection probes capture static objects and the sky into a cubemap, which the water shader samples to reflect the surrounding environment. This method works especially well in our static scenes, such as the pool. In addition, SSR uses screen-space data to add finer reflection details of nearby geometry, enhancing realism as the camera view changes.

To maintain visual clarity, especially in calm areas, we reduced the strength of surface normal distortion in the reflection pass. This helps preserve the integrity of reflection lines while retaining ripple details in lighting and transparency calculations.

#sideBySideImages(
"with_reflection.png",
"without_reflection.png",
caption: [With reflection / without reflection.],
)

By combining static and dynamic reflection techniques, we achieved a balance between performance and visual fidelity across different scenarios.


= Final Result and Conclusion
Our final implementation successfully captures some characteristics of realistic water:
- Wave Motion: Gerstner wave-based vertex animation produces continuous, lifelike wave motion that responds well across both large and small environments.
- Surface Detail: Layered normal maps introduce high-frequency ripples that complement the larger wave forms, creating a natural, uneven surface.
- Transparency and Refraction: Using screen color sampling combined with surface normals, our water surface realistically refracts and distorts the background geometry based on viewing angle and depth.
- Chromatic Dispersion: Simulated light separation adds subtle, color-fringed highlights near submerged object edges, enhancing visual realism.
- Depth-based Opacity and Blur: Depth-dependent modulation of transparency and blur replicates real-world underwater visibility, making objects at greater depths appear more obscured.
- Foam Generation: Procedural foam effects along shallow edges create a soft and convincing interaction between water and nearby geometry, eliminating hard visual boundaries.
- Reflection: Baked probes capture the static environment, while SSR adds real-time reflection details, improving realism without significant performance cost.

Overall, our water rendering solution demonstrates both artistic quality and technical sophistication,
and is flexible to seamlessly adapt to a variety of scenes.



#figure(
  image("final_pool.png"),
  caption: [Final result of pool]
)

= References
- #link("https://www.youtube.com/watch?v=4CNad5V9wD8&t=1139s")[Use compute shaders to create water effect in Unity. Now you can simulate ripples of a moving boat!]
- #link("https://www.youtube.com/playlist?list=PL78XDi0TS4lHBWhZJNOrslnkFWHwE67ak")[Water Shaders Playlist]
- #link("https://assetstore.unity.com/packages/3d/environments/exterior-swimming-pool-161671")[Exterior Swimming Pool 3D model]
- #link("https://dl.acm.org/doi/10.1145/37402.37422")[Marching cubes: A high resolution 3D surface construction algorithm]
- #link("https://www.youtube.com/watch?v=vTMEdHcKgM4&t=65s")[Marching Cubes]

= Appendix

== Terrain Generation
In the early stages of our final project, we planned to develop a terrain generator.
We implemented the Marching Cubes algorithm and leveraged GPU acceleration to ensure fast computation.
Perlin noise was used to generate the terrain, and terrain chunks are dynamically generated.

#figure(
image("marching_cubes.png", width: 30%),
caption: [Our preliminary result of terrain generation]
)

// == Circle Ripples

// #fig-outline()
// #tab-outline()
