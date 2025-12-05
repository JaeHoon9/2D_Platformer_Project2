using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel; // 환경설정 Panel GameObject를 연결할 변수

    // --- 음량 조절 관련 변수 ---
    public AudioMixer gameMixer; // Project 뷰에서 생성한 Audio Mixer를 연결
    public Slider masterVolumeSlider; // 마스터 음량 슬라이더 UI를 연결
    private const string MASTER_VOLUME_PARAM = "MasterVolume"; // Audio Mixer의 Exposed Parameter 이름과 동일하게

    // --- 해상도 조절 관련 변수 ---
    public Dropdown resolutionDropdown; // 해상도 드롭다운 UI를 연결
    private List<Resolution> resolutions; // 현재 시스템에서 지원하는 해상도 목록

    void Start()
    {
        // 환경설정 Panel이 비활성화되어 있는지 확인 (에디터에서 실수로 켜져 있을까봐)
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // --- 음량 설정 초기화 및 로드 ---
        InitializeVolumeSettings();

        // --- 해상도 설정 초기화 및 로드 ---
        InitializeResolutionSettings();
    }

    // --- 환경설정 창 열기/닫기 함수 ---
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true); // Panel 활성화
        }
    }

    public void CloseSettings()
    {
        // 설정 닫기 전에 변경사항 저장
        PlayerPrefs.Save();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false); // Panel 비활성화
        }
    }

    // --- 음량 관련 함수 ---
    private void InitializeVolumeSettings()
    {
        if (masterVolumeSlider == null || gameMixer == null) return;

        if (PlayerPrefs.HasKey(MASTER_VOLUME_PARAM))
        {
            float savedVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_PARAM);
            SetMasterVolume(savedVolume); // Audio Mixer에 적용
            masterVolumeSlider.value = savedVolume; // 슬라이더 UI에 적용
        }
        else
        {
            // 저장된 값이 없으면 기본값 설정 (예: 1.0f, 최댓값)
            SetMasterVolume(1.0f);
            masterVolumeSlider.value = 1.0f;
        }
    }

    public void SetMasterVolume(float value)
    {
        // 슬라이더의 선형 값(0~1)을 Audio Mixer의 로그 스케일(-80dB~0dB)로 변환
        // value가 0일 때 Log10(0)은 무한대가 되므로, 작은 값(0.0001)으로 보정
        float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        gameMixer.SetFloat(MASTER_VOLUME_PARAM, volume);

        // 현재 음량 값을 PlayerPrefs에 저장
        PlayerPrefs.SetFloat(MASTER_VOLUME_PARAM, value);
    }

    // --- 해상도 관련 함수 ---
    private void InitializeResolutionSettings()
    {
        if (resolutionDropdown == null) return;

        resolutions = new List<Resolution>();
        // 현재 모니터가 지원하는 모든 해상도 가져오기
        foreach (Resolution res in Screen.resolutions)
        {
            // 중복 해상도 제거 (같은 해상도에 다른 주사율이 있을 수 있으므로)
            bool found = false;
            foreach (Resolution existingRes in resolutions)
            {
                if (existingRes.width == res.width && existingRes.height == res.height)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                resolutions.Add(res);
            }
        }

        // 높은 해상도부터 보이도록 역순 정렬 (선택 사항)
        resolutions.Reverse();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0; // 드롭다운에 현재 적용된 해상도를 표시하기 위한 인덱스

        for (int i = 0; i < resolutions.Count; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            // 현재 시스템 해상도와 일치하는 해상도를 찾아서 기본 선택값으로 설정
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.ClearOptions(); // 기존 옵션 초기화
        resolutionDropdown.AddOptions(options); // 새 옵션 추가
        resolutionDropdown.value = currentResolutionIndex; // 현재 해상도로 초기 선택
        resolutionDropdown.RefreshShownValue(); // 드롭다운 UI 업데이트

        // 저장된 해상도 설정 로드
        if (PlayerPrefs.HasKey("resolutionIndex"))
        {
            int savedResolutionIndex = PlayerPrefs.GetInt("resolutionIndex");
            if (savedResolutionIndex >= 0 && savedResolutionIndex < resolutions.Count)
            {
                SetResolution(savedResolutionIndex); // 저장된 해상도로 적용
                resolutionDropdown.value = savedResolutionIndex; // 드롭다운 UI에도 적용
                resolutionDropdown.RefreshShownValue();
            }
        }
        else
        {
            // 저장된 값이 없으면 현재 해상도로 설정
            SetResolution(currentResolutionIndex);
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= resolutions.Count) return;

        Resolution resolution = resolutions[resolutionIndex];
        // Screen.fullScreen은 현재 풀스크린 모드 여부를 유지합니다.
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        // 해상도 인덱스를 PlayerPrefs에 저장
        PlayerPrefs.SetInt("resolutionIndex", resolutionIndex);
    }
}