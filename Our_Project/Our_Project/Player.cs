﻿using Our_Project.States_and_state_related;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our_Project
{
    public class Player
    {
        public int army_size;
        public Pawn[] pawns;
        public bool myTurn=false;
        public Board Board
        {
            get
            {
                return buildingBoardState.getEmptyBoard();
            }
        }
        public BuildingBoardState buildingBoardState;
        public Player(Game game)
        {
            buildingBoardState = (BuildingBoardState)game.Services.GetService(typeof(IBuildingBoardState));
           
            army_size = 21;
        }
    }
}
