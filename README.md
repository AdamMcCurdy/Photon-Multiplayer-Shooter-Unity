# ACEXR Test
## SINGLE PLAYER AND MULTIPLAYER REUSABLE OBJECTS

> [!NOTE]
> I spent about 2 hours on this test this weekend, I feel there are many areas to work on.
> Network authority needs to be exchanged when players hit targets
> Lighting seems duplicated or too bright at the very least
> Network latency frame prediction / compensation not implemented
> Color change doesn't happen on spawn, but does happen on using E key.
> Currently space and mouse button triggers shoot event. 

## Task
This interview task involves replicating the hit detection and reset function of an existing core
feature of Ace VR creating a scene of targets with hit detection that can be traced to the
player that hit them in a reusable way.
- Set up a new Unity project using version 2022.3.20f1
- Setup the project/scene for Photon fusion or PUN2 - https://doc.photonengine.com
- Create a basic player - a moveable capsule using basic keyboard inputs with a ray
coming out the front
- Create 2 scenes
    - Scene 1 is Single Player and should have no enabled networked object components and includes a single player
    - Scene 2 is Multiplayer and when running should automatically spawn 1 player per device joining the same test lobby
    - Both scenes upon running should instantiate as similar as possible prefab “targets” (6-10 basic 1 m white cubes) with hit detection that change color when “hit” (a hit is detected when the ray of a player is crossing with the target while the player presses the spacebar)
    - Hit targets should change color based on which player hit them (green for player1/single player, red for player2, etc) for all players to see for about 5 seconds before returning to their original state

● Share this project by inviting Chris (@ChrisAceXR) and Aaron (@blobworks) to be
collaborators on a new private Github repository, the project should be ready to go
upon fetch and playable within Unity editor. It will be tested on PC.

Feel free to add more depth or adjustable parameters in the editor if deemed helpful, as well
as any comments for choices and designs made within the script(s), everything made here
will be discussed in the panel interview that follows once the above is completed. We don’t
expect you to take more than a few hours on this and are not expecting anything to be pretty
or polished for this so even if you only partially complete it feel free to submit what you have.
*All code / assets created for any of Ace’s interview assignments are strictly for the purpose
of assessing the candidate, it will not be repurposed or reused for any other purposes
without the involvement and consent of the candidate.
