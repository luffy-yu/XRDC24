# Float Mind

## Inspiration

In our fast-paced world, stress and emotional disconnect are widespread. To address this, we explored how new technologies could help stressed individuals relax and recharge quickly. Inspired by mindfulness and visual-based meditation, our team developed a gamified meditation approach. This helps users relax their bodies and minds while visualizing and overcoming emotional challenges. Our solution blends gamification, AR/VR, and AI to make mental wellness practices more engaging and impactful.

## What it does

**Float Mind** is an AI-powered AR/VR meditation tool designed to help stressed individuals effectively relax and reconnect with their emotions through an immersive mindfulness experience. With a seamless, controller-free interface on Meta Quest, it offers a unique combination of AI-driven emotional insights and interactive meditation practices.
The experience begins with Flo, an empathetic AI companion that analyzes users’ thoughts and visualizes their positive and negative emotions as interactive 3D bubbles. Through intuitive hand gestures, users engage with these bubbles, dynamically shaping their virtual environment. This transitions seamlessly into a guided meditation phase, where calming breathing exercises, soothing animations, and interactive elements like growing auroras and nurturing trees promote mindfulness and relaxation.

## Why it matters

**Float Mind** integrates cutting-edge AI with immersive technology to redefine traditional mindfulness practices. It provides a trusted, science-backed mental wellness solution, offering busy professionals an accessible and impactful way to manage stress and enhance emotional well-being, whether at work or at home.

## How we built it

**Float Mind** was developed using Meta Quest 2, Unity 6, and advanced AI technologies. The emotional analysis feature leverages LLM Agent (GPT-o1 mini model) to detect positivity and negativity in user input, while the AR/VR experience was crafted using Oculus XR All-in-One SDK. Our team combined expertise in AI, UI/UX design, and immersive 3D environments to create a seamless and engaging user journey. Rigorous testing ensured the interactivity and flow worked smoothly without the need for controllers.

## Challenges we ran into

* Device constraints: eye tracking feature requires Oculus Quest Pro headset.
* Large scene rendering latency in the headset, need to add optimization (eg. use lower mesh 3d models, less particles, bake environment lighting, etc.)
* Inhale/exhale effect implementation
* Portal effect implementation to bridge the virtuality and reality
* Blender geometry node animation compatibility with Unity
* Figma Converter compatibility with Unity.

## Accomplishments that we're proud of

We're proud to create an immersive and gamified meditation tool that embraces cutting-edge AI and AR/VR technology, grounded in the science of mindfulness and psychology. We crafted a visually stunning 3D environment with intuitive spatial interactions and audio-reactive animation.

Significant technical achievements include the successful integration of LLM-driven emotional analysis with AR/VR interactions and the delivery of a seamless controller-free VR headset experience. The core immersive interactions rely on hand tracking, combined with hand gesture detection.

**Bridge AR and VR through User Actions**
Through thoughtful UX design, we empowered users to seamlessly bridge the gap between AR and VR experiences, amplifying participatory joy.

**Interactive 3D CTA**
We innovated by replacing traditional 2D CTAs with a 3D cube, allowing users to proceed, revert actions, or engage with the AI agent all within a single interactive element.

**Meditation Therapy LLM Agent**
We developed an LLM-powered agent designed specifically for meditation therapy, providing dynamic, personalized guidance and emotional support.

**Multi-Modal Input via Hand Gesture Detection**
Using advanced hand tracking, we recognized gestures—such as index-finger poking, double-hand waving, and pushing—to enable intuitive, controller-free interactions.

**Audio-Driven Prompting and Mood Detection**
By feeding audio input to the LLM agent, we gauged positive or negative user moods in real time, generating responsive, interactable “bubbles” to enhance engagement.

**Seamless Scene Blending with Oculus Scene API**
We harnessed the Oculus Scene API to capture physical-world meshes, effortlessly merging real-world surroundings with the virtual scene during the bubble interaction phase.

**Dissolving Transition Effect**
A gradual dissolving effect transitions users from the real world to the virtual environment, maintaining immersion and ensuring a smooth user experience.

**AI Agent Avatar with Real-Time Audio**
We incorporated text-to-speech for the agent’s GPT-generated responses, supported by real-time audio reflection to enrich interactivity and bring the avatar to life.

## What we learned

* XR+UX
* XR+AI
* Integrating diverse multi-disciplinary tools from design to development, 2d to 3d, AI XR integration.



## What's next for Float Mind

* Expand Float Mind’s emotional analysis capabilities to recognize a broader range of sentiments and introduce more interactive meditation elements.
* Integrate biofeedback mechanisms to make the experience even more personalized.
* Explore partnerships with mental health professionals and wellness organizations to bring Float Mind to a wider audience.

