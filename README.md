# MATCH

Welcome to the MATCH (Mixed reAliTy Cognitive ortHosis) project!

<b>Before reading this section, it is worth noting that the software's architecture, functionalities and this README are not yet stable ... indeed, some of the software's basis are currently being redesigned. If you are interested in using the software, I advise you to contact me first so that I can provide you with the current status.</b>

This readme has two sections:
- Context
- Install and run the project

## Context

### Why this project?
Mixed reality is an emerging technology aimed at blending virtual content with the real environment. Using a headset, which can be compared to glasses, it allows virtual content to be displayed at any time, anywhere.
People at the beginning of the dementia continuum are often able and wish to live independently at home. But they may have difficulty finding a solution to satisfy certain needs, planning this solution, or taking the steps to carry it out. These difficulties may impact their ability to continue to live independently at home.
To support independence at home for people at the beginning of the dementia continuum, we are investigating the utility of using a mixed reality headset as a cognitive orthosis. There are several theoretical advantages to using this technology in this context: the headset is able to interpret the context of the person and provide the necessary assistance when needed with appropriate gradation (to stimulate their residual cognitive potential); the assistance is provided in the user’s environment, so they do not need to look away (to a screen for example); the environment does not need to be modified to use it. The design of the cognitive orthosis is based on a user-centered design methodology and follows the “zero effort technologies” principles.

### In which context this project is developed
This project is part of my PhD degree, performed within the [DOMUS lab](https://domus.recherche.usherbrooke.ca/) of the the [Université de Sherbrooke](https://www.usherbrooke.ca/about/), and the [Centre universitaire de gériatrie de l'université de Montréal](https://criugm.qc.ca/en/).

### Components and functionalities of the project
There are indeed components and functionalities. However, what is missing are explanations to introduce them. Those explanations will be present here, one day. 
I still need to do this. But in the meantime, if you need any information, please do not hesitate to contact me (see section below).

## Install and run

### Hardware and software requirements
- Microsoft Hololens 2 to deploy the projet (otherwise, you can test some of the functionalities in the unity editor)
- Unity 2020.3.37f1, with the following assets:
  - https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347
  - https://assetstore.unity.com/packages/tools/ai/npbehave-event-driven-code-based-behaviour-trees-75884
  - To have textures when running the tool in the Unity editor: https://assetstore.unity.com/packages/2d/textures-materials/wood/high-quality-realistic-wood-textures-mega-pack-75831
- Visual Studio 2019, with the following component:
  - C++ desktop development 
  - Unity development
  - Development for the universal platform
  - Latest version of MSVC v142 for ARM
  - Windows development kit SDK 10.0.18362.0

### Run it
Here are the steps to make the project running:
- Get the project and make sure you use the main branch
- Open the projet with Unity 2020.3.37f1. 
- If not opened, open the MATCH scene
- Run it in the editor or export it to the Hololens. If you run it in the editor, a "virtual room" is enable to give a bit of context. If you export it for the hololens, do not forget to configure the export correctly with the "universal windows platform" (see the section "Configure your project for Windows Mixed Reality" here: https://docs.microsoft.com/en-us/learn/modules/learn-mrtk-tutorials/1-3-exercise-configure-unity-for-windows-mixed-reality?ns-enrollment-type=learningpath&ns-enrollment-id=learn.azure.beginner-hololens-2-tutorials&tabs=openxr).

### How to use it?
Well, good point. I still need to make a tutorial. If you need any help / information, please do not hesitate to contact me (see section below).

## Contact me
I will be happy to provide you with more information about the software. Also, please do not hesitate to share any feedback you may have about this project.
You can find my contact information here: https://domus.recherche.usherbrooke.ca/guillaume-spalla/.
