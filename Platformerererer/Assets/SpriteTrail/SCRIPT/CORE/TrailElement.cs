using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrailElement : MonoBehaviour 
{
    public static Stack<GameObject> m_FreeElements = new Stack<GameObject>();

    public static int m_ItemCount = 0;
    public int m_ItemId = -1;
    TrailPreset m_TrailSettings;
    float m_TimeSinceCreation;
    SpriteRenderer m_SpRenderer;
    bool m_Init = false;
    Vector3 m_InitSize;
    Vector3 m_InitPos;
    SpriteTrail m_MotherTrail;
    public bool m_NeedLateUpdate = false;
    public int m_TrailPos = -1;
    bool m_NeedDequeue = false;
    public Transform m_Transform;

	public static void ClearFreeElements()
	{
		while(m_FreeElements.Count > 0)
		{
			Destroy(m_FreeElements.Pop().gameObject);
		}
	}

    public void Initialise(SpriteTrail trail)
    {
        m_NeedDequeue = false;
        m_TrailPos = -1;
        m_NeedLateUpdate = false;
        m_TimeSinceCreation = 0;
        m_MotherTrail = trail;
        this.gameObject.SetActive(true);
        m_TrailSettings = trail.m_CurrentTrailPreset;
        m_SpRenderer = GetComponent<SpriteRenderer>();
        if (m_TrailSettings.m_SpecialMat != null)
            m_SpRenderer.material = m_TrailSettings.m_SpecialMat;
        else
            m_SpRenderer.material = trail.m_SpriteToDuplicate.material;
        m_SpRenderer.color = trail.m_SpriteToDuplicate.color;
        m_SpRenderer.sortingOrder = trail.m_TrailOrderInLayer;
        m_SpRenderer.sprite = trail.m_SpriteToDuplicate.sprite;
        m_SpRenderer.flipX = trail.m_SpriteToDuplicate.flipX;
        m_SpRenderer.flipY = trail.m_SpriteToDuplicate.flipY;
        m_InitSize = m_Transform.localScale;
        m_InitPos = m_Transform.localPosition;
        m_Init = true;
        trail.m_ElementsInTrail.Enqueue(this);
        ApplyFrameEffect();
        

        if(m_TrailSettings.m_TrailElementDurationCondition == TrailElementDurationCondition.ElementCount)
        {
            if (m_TrailSettings.m_TrailMaxLength > 0)
            {
                while (trail.m_ElementsInTrail.Count > m_TrailSettings.m_TrailMaxLength)
				{
					trail.m_ElementsInTrail.Dequeue().Hide();
				}
            }
            else
            {
                while (trail.m_ElementsInTrail.Count > 0)
                {
                    trail.m_ElementsInTrail.Dequeue().Hide();
                }
            }


            int _cnt = 0;
            foreach (TrailElement _elem in trail.m_ElementsInTrail)
            {
                _elem.m_TrailPos = trail.m_ElementsInTrail.Count - _cnt;
                _elem.m_NeedLateUpdate = true;
                _cnt++;
            }
        }
    }

    private void Awake()
    {
        m_Transform = transform;
        m_ItemId = m_ItemCount;
        m_ItemCount++;
    }

    private void Update()
    {
        if (!m_Init) return;
        m_TimeSinceCreation += Time.deltaTime;
        ApplyFrameEffect();
    }

    private void LateUpdate()
    {
        if (!m_NeedLateUpdate) return;
        ApplyAddSpriteEffect(m_TrailPos);
        m_NeedLateUpdate = false;
    }

    void ApplyAddSpriteEffect(int index)
    {
        if(m_TrailSettings.m_TrailMaxLength > 0)
        {
            ApplyModificationFromRatio(/*1f - */((float)index / (float)m_TrailSettings.m_TrailMaxLength));
        }
            
    }

    void ApplyFrameEffect()
    {
        float _Ratio = 0;
        if (m_TrailSettings.m_TrailDuration > 0)
            _Ratio = m_TimeSinceCreation / m_TrailSettings.m_TrailDuration;

        if(_Ratio >= 1)
        {
            Hide();
            return;
        }
        if(m_TrailSettings.m_TrailElementDurationCondition == TrailElementDurationCondition.Time)
            ApplyModificationFromRatio(_Ratio);
    }

    void ApplyModificationFromRatio(float ratio)
    {
        if (m_TrailSettings.m_UseOnlyAlpha)
        {
            Color _tmpCol = m_SpRenderer.color;
            _tmpCol.a = m_TrailSettings.m_TrailColor.Evaluate(ratio).a;
            m_SpRenderer.color = _tmpCol;
        }
        else
            m_SpRenderer.color = m_TrailSettings.m_TrailColor.Evaluate(ratio);

        if (m_TrailSettings.m_UseSizeModifier)
        {
            Vector3 _NewSize = new Vector3();
            _NewSize.x = m_InitSize.x * m_TrailSettings.m_TrailSizeX.Evaluate(ratio);
            _NewSize.y = m_InitSize.y * m_TrailSettings.m_TrailSizeY.Evaluate(ratio);
            _NewSize.z = 1f;
            m_Transform.localScale = _NewSize;
        }
        if (m_TrailSettings.m_UsePositionModifier)
        {
            Vector3 _NewPos = m_InitPos;
            _NewPos.x += m_TrailSettings.m_TrailPositionX.Evaluate(ratio);
            _NewPos.y += m_TrailSettings.m_TrailPositionY.Evaluate(ratio);
            m_Transform.localPosition = _NewPos;
        }
    }

    public static GameObject GetFreeElement()
    {
        if (m_FreeElements.Count > 0)
        {
            return m_FreeElements.Pop();
        }
        return GameObject.Instantiate(SpriteTrail.GetTrailElementPrefab());
    }

    public void Hide(bool AddToFree = true)
    {
        if (gameObject == null || this == null) return;
        if(m_MotherTrail != null)
            m_NeedDequeue = true;
        if (m_MotherTrail != null && m_MotherTrail.m_ElementsInTrail.Count > 0 && m_MotherTrail.m_ElementsInTrail.Peek().m_NeedDequeue)
            m_MotherTrail.m_ElementsInTrail.Dequeue();
        this.gameObject.SetActive(false);
        if(AddToFree)
            m_FreeElements.Push(this.gameObject);
        m_Init = false;
    }
}
