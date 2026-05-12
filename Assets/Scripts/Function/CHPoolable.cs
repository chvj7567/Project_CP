// 글로벌 CHPoolable은 패키지 ChvjUnityInfra.CHPoolable을 상속해 prefab의 script reference(GUID)를 보존하면서
// 패키지 CHMPool의 GetComponent<ChvjUnityInfra.CHPoolable>() 검색에 대응시킨다.
// P9에서 prefab GUID를 패키지로 재바인딩한 후 이 파일을 삭제 예정.
public class CHPoolable : ChvjUnityInfra.CHPoolable { }
