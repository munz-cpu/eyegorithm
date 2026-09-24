using System.Collections.Generic;

public sealed class 급식실대기열
{
    private readonly List<학생정보> 학생들 = new();

    public int 인원수 => 학생들.Count;
    public IReadOnlyList<학생정보> 목록 => 학생들;

    public void 추가(학생정보 학생)
    {
        if (학생 != null && !학생들.Contains(학생))
            학생들.Add(학생);
    }

    public void 제거(학생정보 학생)
    {
        학생들.Remove(학생);
    }

    public void 비우기()
    {
        학생들.Clear();
    }

    // 현재 스케줄링 방식에 따라 다음에 급식실로 들어갈 학생을 고른다.
    public 학생정보 다음학생선택(큐_타입 큐종류)
    {
        if (학생들.Count == 0)
            return null;

        int 선택번호 = 0;

        if (큐종류 == 큐_타입.최소실행시간우선)
        {
            for (int i = 1; i < 학생들.Count; i++)
            {
                if (학생들[i].남은시간 < 학생들[선택번호].남은시간)
                    선택번호 = i;
            }
        }
        else if (큐종류 == 큐_타입.우선순위)
        {
            for (int i = 1; i < 학생들.Count; i++)
            {
                if ((int)학생들[i].계급값 > (int)학생들[선택번호].계급값)
                    선택번호 = i;
            }
        }

        학생정보 선택학생 = 학생들[선택번호];
        학생들.RemoveAt(선택번호);
        return 선택학생;
    }

    public int 위치찾기(학생정보 학생)
    {
        return 학생들.IndexOf(학생);
    }
}
