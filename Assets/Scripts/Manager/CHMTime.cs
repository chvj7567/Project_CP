using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

// NTP 기반 서버 시각 매니저. 디바이스 시각 위변조 방지를 위해 첫 진입 시 NTP를 1회 받고
// 이후엔 Time.realtimeSinceStartup 경과로 보정해 사용한다. 콜드 부팅 시 NTP 재조회 필수.
public class CHMTime : ChvjUnityInfra.CHSingletonStatic<CHMTime>
{
    // 외부 노출
    public bool IsAvailable { get; private set; }
    public Subject<Unit> OnAvailable { get; } = new Subject<Unit>();

    // NTP 응답 절대 시각 (UTC)
    DateTime _lastNtpUtc;
    // 응답 받은 시점의 realtimeSinceStartup
    float _capturedRealtime;

    const string NtpServer1 = "time.google.com";
    const string NtpServer2 = "pool.ntp.org";
    const int NtpTimeoutMs = 3000;
    const int MaxRetries = 5;
    const float RetryIntervalSec = 30f;
    const int NtpPacketSize = 48;                  // SNTP 패킷 고정 크기 (RFC 4330)
    const int NtpPort = 123;                       // NTP 표준 UDP 포트
    const ulong NtpFractionDenominator = 0x100000000UL; // 2^32 — NTP 분수부 → 밀리초 변환용 분모

    // 첫 NTP 조회 — 실패해도 즉시 완료. 백그라운드 재시도가 IsAvailable을 갱신.
    public async Task Init()
    {
        bool ok = await TryFetchNtpAsync();
        if (!ok)
        {
            // 백그라운드 재시도 — fire-and-forget Task
            _ = RetryLoopAsync();
        }
    }

    async Task RetryLoopAsync()
    {
        for (int i = 0; i < MaxRetries; ++i)
        {
            await Task.Delay(TimeSpan.FromSeconds(RetryIntervalSec));
            if (IsAvailable) return;
            bool ok = await TryFetchNtpAsync();
            if (ok) return;
        }
        Debug.LogWarning("[CHMTime] NTP 재시도 모두 실패. 일일 미션 잠금 유지.");
    }

    async Task<bool> TryFetchNtpAsync()
    {
        try
        {
            var utc = await FetchNtpAsync(NtpServer1);
            ApplyNtp(utc);
            return true;
        }
        catch (Exception e1)
        {
            Debug.LogWarning($"[CHMTime] {NtpServer1} 실패: {e1.Message}. 폴백 시도");
            try
            {
                var utc = await FetchNtpAsync(NtpServer2);
                ApplyNtp(utc);
                return true;
            }
            catch (Exception e2)
            {
                Debug.LogWarning($"[CHMTime] {NtpServer2} 실패: {e2.Message}");
                return false;
            }
        }
    }

    // 주 스레드 호출 전제 — Init()이 주 스레드에서 호출되면 Unity의 SynchronizationContext가
    // await 연속을 주 스레드로 라우팅. OnAvailable.OnNext의 UniRx 구독자가 UI/GameObject에
    // 접근해도 안전. 만약 다른 스레드에서 호출하는 케이스가 추가되면 MainThreadDispatcher 경유 필요.
    void ApplyNtp(DateTime utc)
    {
        _lastNtpUtc = utc;
        _capturedRealtime = Time.realtimeSinceStartup;
        bool wasAvailable = IsAvailable;
        IsAvailable = true;
        if (!wasAvailable) OnAvailable.OnNext(Unit.Default);
        Debug.Log($"[CHMTime] NTP 동기화 완료 UTC: {utc:yyyy-MM-dd HH:mm:ss}");
    }

    // SNTP 1회 조회. UDP 123 포트, 48바이트 패킷.
    static async Task<DateTime> FetchNtpAsync(string host)
    {
        var data = new byte[NtpPacketSize];
        data[0] = 0x1B; // LI=0, VN=3, Mode=3 (client)

        var addresses = await Dns.GetHostAddressesAsync(host);
        if (addresses.Length == 0) throw new Exception("DNS 실패");
        var endpoint = new IPEndPoint(addresses[0], NtpPort);

        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.ReceiveTimeout = NtpTimeoutMs;
            socket.SendTimeout = NtpTimeoutMs;
            await Task.Run(() => socket.SendTo(data, endpoint));
            await Task.Run(() => socket.Receive(data));
        }

        // Transmit Timestamp: 바이트 40–47
        const byte offsetTransmitTime = 40;
        ulong intPart = ((ulong)data[offsetTransmitTime + 0] << 24)
                      | ((ulong)data[offsetTransmitTime + 1] << 16)
                      | ((ulong)data[offsetTransmitTime + 2] << 8)
                      | ((ulong)data[offsetTransmitTime + 3]);
        ulong fracPart = ((ulong)data[offsetTransmitTime + 4] << 24)
                       | ((ulong)data[offsetTransmitTime + 5] << 16)
                       | ((ulong)data[offsetTransmitTime + 6] << 8)
                       | ((ulong)data[offsetTransmitTime + 7]);

        ulong milliseconds = (intPart * 1000UL) + ((fracPart * 1000UL) / NtpFractionDenominator);
        var networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);
        return networkDateTime;
    }

    // 현재 UTC 시각 (NTP + realtimeSinceStartup 경과)
    public DateTime UtcNow
    {
        get
        {
            if (!IsAvailable) return DateTime.UtcNow; // 폴백 — 호출자가 IsAvailable 체크해야 함
            return _lastNtpUtc.AddSeconds(Time.realtimeSinceStartup - _capturedRealtime);
        }
    }

    // 일일 미션 리셋 키 ("yyyyMMdd" UTC)
    public string GetUtcDateKey() => UtcNow.ToString("yyyyMMdd");

    // 다음 UTC 자정까지 남은 초
    public double GetSecondsUntilNextUtcMidnight()
    {
        var now = UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        return (nextMidnight - now).TotalSeconds;
    }
}
