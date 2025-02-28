using MiraAPI.GameEnd;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Example.GameOver;

public class NeutralKillerGameOver : CustomGameOver
{
    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.WinText.text = "Outcast Killer Wins!";
        endGameManager.WinText.color = Color.magenta;
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, Color.magenta);
    }
}
