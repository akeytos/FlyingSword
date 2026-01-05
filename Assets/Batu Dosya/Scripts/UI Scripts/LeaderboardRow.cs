using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public RawImage avatarImage;

    public void SetData(int rank, string username, int score, Texture2D avatar)
    {
        rankText.text = "#" + rank.ToString();
        nameText.text = username;
        scoreText.text = score.ToString("N0"); // 10,000 gibi virgüllü yazar

        if (avatar != null)
        {
            avatarImage.texture = avatar;
            avatarImage.color = Color.white;
        }
    }
}