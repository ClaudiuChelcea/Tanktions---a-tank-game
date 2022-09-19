# Tanktions---a-tank-game

## Project Description
This has been our final project for the Rapid Game Development course in 3DUPB 2022 Summer School. It is a tank game inspired by the famous Scorched Tanks, but with a twist that is similar to [Graphwar](https://github.com/catabriga/graphwar). The twist is related to the shooting mechanic, which is controlled by giving a function of the x coordinate as input and the bullet will follow its graph as its trajectory. This idea can give rise to a higher skill ceiling and it is good for practicing Maths and developing an intuition about how functions work and how to manipulate them, a skill required in engineering.

The game was developed in Unity, using the Netcode for GameObjects package to enable multiplayer.

## Game Mechanics
The game is turn based, and there are a handful of game states and logic that define how the game evolves. The states are enumerated below:
- Waiting: this state is triggered once the host has loaded the arena, and it stays in this state until the client has loaded as well
- Moving: in this state, the current player has 5 seconds to move its tank
- Aiming: in this state, the current player has 35 seconds to input the function that the bullet will follow
- Shooting: in this state, the bullet is fired and it has 15 seconds to reach its target, or collide with the obstacles
- Interrupted: this state is triggered when the other player disconnected, and will lead to a return to the main menu after 2 seconds
- Ended: this state is triggered when the game has ended (by the death of one player), and the winner is showed, followed by a return to the main menu after 2 seconds.

Currently, a bullet hit deals 50hp of damage and one player has a total of 100hp. This means that 2 hits are necessary to defeat the opponent.
The specific values may be changed in the future.

## Gallery
![image](https://user-images.githubusercontent.com/74200190/189699765-3c61db9b-5f1c-4c69-8ed0-ba8219ec088a.png)
![image](https://user-images.githubusercontent.com/74200190/189699822-54361bc1-35c9-4438-b464-566d5b3399ef.png)
![image](https://user-images.githubusercontent.com/74200190/189699952-250d6901-2de6-4e11-8dd9-15af8425a462.png)
![image](https://user-images.githubusercontent.com/53219413/190963389-7d876e58-317e-4116-be15-db57e43b0f97.png)

