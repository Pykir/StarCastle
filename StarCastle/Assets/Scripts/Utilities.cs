using System;
using UnityEngine;

public static class Utilities
{
    public enum GameState
    {
        Play,
        Pause,
        Intermission,
        Respawning,
        GameOver
    }

    public enum MainMenuState
    {
        MainMenu,
        Credits,
        HowToPlay,
        Settings
    }
}