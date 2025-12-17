using UnityEngine;

public class GameResultUI : MonoBehaviour
{
    // 버튼과 연결될 함수입니다.
    public void OnExitClick()
    {
        // 에디터에서 실행 중일 때는 플레이 모드를 멈춤
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 실제 빌드된 게임에서는 어플리케이션 종료
            Application.Quit();
#endif
    }
}