using UnityEngine;
using System.Collections;

/// <summary>
/// 파티클 자동 파괴 컴포넌트
/// 파티클이 날아가며 페이드 아웃 후 자동으로 파괴됩니다
/// </summary>
public class ParticleAutoDestroy : MonoBehaviour
{
    private SpriteRenderer _sr;
    
    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// 파티클 날아가기 시작
    /// </summary>
    public void StartFloat(Vector3 offset, float duration)
    {
        StartCoroutine(CoFloat(offset, duration));
    }
    
    private IEnumerator CoFloat(Vector3 offset, float duration)
    {
        if (_sr == null)
        {
            Destroy(gameObject);
            yield break;
        }
        
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + offset;
        Color startColor = _sr.color;
        
        float startRotation = transform.eulerAngles.z;
        float rotationSpeed = Random.Range(-720f, 720f); // 빠른 회전
        
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            if (this == null || gameObject == null) yield break;
            
            float p = t / duration;
            
            // 포물선 움직임 (위로 튕김) - 기존 0.3f → 0.55f
            transform.position = Vector3.Lerp(startPos, endPos, p);
            transform.position += Vector3.up * (Mathf.Sin(p * Mathf.PI) * 0.55f);
            
            // 회전
            transform.eulerAngles = new Vector3(0, 0, startRotation + rotationSpeed * t);
            
            // 페이드 아웃 (ease-in-out)
            float alpha = 1f - Mathf.Pow(p, 1.5f);
            _sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            yield return null;
        }
        
        // 애니메이션 완료 후 파괴
        Destroy(gameObject);
    }
}
