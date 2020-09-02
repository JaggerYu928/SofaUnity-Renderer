# -*- coding: utf-8 -*-
from stlib.scene import MainHeader, ContactHeader
from stlib.physics.rigid import Floor, Cube
from gripper import Gripper

def createScene(rootNode):
    """This is my first scene"""
    MainHeader(rootNode, gravity=[0.0, -981.0, 0.0], plugins=["SoftRobots"])
    ContactHeader(rootNode, alarmDistance=2, contactDistance=1, frictionCoef=0.08)

    Gripper(rootNode)

    Floor(rootNode, name="Floor",
          color=[1.0,0.0,0.0],
          translation=[0.0,-260.0,0.0],
          isAStaticObject=True)
          
    Cube(rootNode, name="Cube",
          uniformScale=20.0,
          color=[1.0,1.0,0.0],
          totalMass=0.03,
          volume=20,
          inertiaMatrix=[1000.0,0.0,0.0,0.0,1000.0,0.0,0.0,0.0,1000.0],
          translation=[0.0,-130.0,10.0])

    return rootNode
