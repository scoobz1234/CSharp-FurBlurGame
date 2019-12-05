#define HIDE_TRAIL_IN_HIERRARCHY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrailActivationCondition
{
    AlwaysEnabled,
    Manual,
    VelocityMagnitude
    //    Acceleration
}

public enum TrailDisactivationCondition
{
    Manual,
    VelocityMagnitude,
    Time
}

public class SpriteTrail : MonoBehaviour 
{
    [Tooltip("Trail name")]
    public string m_TrailName = "";
    [Tooltip("The current trail settings that is used.")]
    public TrailPreset m_CurrentTrailPreset;
    [Tooltip("Parent of the trail elements. If the field is empty, it will create the trail in world space. Can be usefull if the creator of the trail is in a mobile element so the trail is created in local space \n Can be set with SetTrailParent(Transform trailParent)")]
    [SerializeField]
    private Transform m_TrailParent;
    [Tooltip("Specify the sprite renderer that will be used as a base for the trail. You can leave it empty if the script is on the same gameobject as the sprite renderer")]
    public SpriteRenderer m_SpriteToDuplicate;
    [Tooltip("Use this to specify the layer name of the trail elements")]
    public string m_LayerName = "Default";
    [Tooltip("Modify the z-Pos of the sprite to avoid overlapping")]
    public float m_ZMoveStep = 0.0001f;
    [Tooltip("max z-Pos allowed for the sprites pos")]
    public float m_ZMoveMax = .9999f;
    [Tooltip("Set a specific order in layer")]
    public int m_TrailOrderInLayer = 0;
    [Tooltip("Check this if you want the trail to be hide if it is disabled")]
    public bool m_HideTrailOnDisabled = true;
    [Tooltip("The requirement to start the trail")]
    public TrailActivationCondition m_TrailActivationCondition = TrailActivationCondition.AlwaysEnabled;
    [Tooltip("The requirement to stop the trail")]
    public TrailDisactivationCondition m_TrailDisactivationCondition = TrailDisactivationCondition.Manual;
    [Tooltip("Check this if you want to start the trail only if the velocity is under the limit")]
    public bool m_StartIfUnderVelocity = false;
    [Tooltip("Check this if the velocity is in local space")]
    public bool m_VelocityStartIsLocalSpace = false;
    [Tooltip("The minimum velocity needed to start the trail")]
    public float m_VelocityNeededToStart;
    [Tooltip("Check this if you want to stop the trail only if the velocity is over the limit")]
    public bool m_StopIfOverVelocity = false;
    [Tooltip("Check this if the velocity is in local space")]
    public bool m_VelocityStopIsLocalSpace = false;
    [Tooltip("The minimum velocity needed to stop the trail")]
    public float m_VelocityNeededToStop;
    [Tooltip("The duration of the trail activation (in seconds). After this delay, the trail is automatically disabled")]
    public float m_TrailActivationDuration;
    

    public Queue<TrailElement>  m_ElementsInTrail =         new Queue<TrailElement>();

	public static int m_TrailCount = 0;

    private TrailPreset         m_PreviousTrailPreset;
    private static GameObject   m_TrailElementPrefab;
    private Transform           m_LocalTrailContainer;
    private Transform           m_GlobalTrailContainer;
    private float               m_CurrentDisplacement;
    private float               m_TimeTrailStarted;    
    private float               m_PreviousTimeSpawned;
    private int                 m_PreviousFrameSpawned;
    private Vector2             m_PreviousPosSpawned;
    private Vector2             m_PreviousFrameLocalPos;
    private Vector2             m_PreviousFrameWorldPos;
    private Vector2             m_VelocityLocal;
    private Vector2             m_VelocityWorld;
    private bool                m_FirstVelocityCheck =          false;
    private bool                m_CanBeAutomaticallyActivated = true;
    private bool                m_EffectEnabled;
    private bool m_WillBeActivatedOnEnable = false;
	public static bool m_LevelRefreshDone = false;

    #region PUBLIC
    /// <summary>
    /// Set the trail parent if your item is in another moving item, and you want the trail to be in local space
    /// -- Example : your character enters a moving train : you probably want the trail to be in the train local space, so SetTrailParent(TheTrainTransform).
    /// </summary>
    /// /// <param name="trailParent">The parent transform. Set it to null if you want it in world space</param>
    public void SetTrailParent(Transform trailParent)
    {
        m_TrailParent = trailParent;
        if (m_TrailParent == null)
        {
            GameObject _tmpGo = GameObject.Find("GlobalTrailContainer");

            if (_tmpGo == null)
            {
                m_GlobalTrailContainer = new GameObject("GlobalTrailContainer").transform;
#if HIDE_TRAIL_IN_HIERRARCHY
                m_GlobalTrailContainer.hideFlags = HideFlags.HideInHierarchy;
#endif
            }
            else
            {
                m_GlobalTrailContainer = GameObject.Find("GlobalTrailContainer").transform;
            }
            return;
        }
        Transform _tmpTrans = trailParent.FindChild("LocalTrailContainer");
        if (_tmpTrans == null)
        {
            m_LocalTrailContainer = new GameObject("LocalTrailContainer").transform;
#if HIDE_TRAIL_IN_HIERRARCHY
            m_LocalTrailContainer.hideFlags = HideFlags.HideInHierarchy;
#endif
            m_LocalTrailContainer.transform.SetParent(m_TrailParent, false);
        }
        else
        {
            m_LocalTrailContainer = _tmpTrans;
        }
    }

    /// <summary>
    /// Modify the trail effect
    /// </summary>
    /// /// <param name="preset">The trail preset you want to set</param>
    public void SetTrailPreset(TrailPreset preset)
    {
        m_PreviousTrailPreset = m_CurrentTrailPreset;
        m_CurrentTrailPreset = preset;
    }


    /// <summary>
    /// Trail start
    /// </summary>
    public void EnableTrail()
    {
        if (!gameObject.activeInHierarchy) return;
        m_CanBeAutomaticallyActivated = true;
        m_PreviousPosSpawned = m_SpriteToDuplicate.transform.position;
        switch (m_TrailActivationCondition)
        {
            case TrailActivationCondition.Manual:
                EnableTrailEffect();
                break;
            case TrailActivationCondition.AlwaysEnabled:
                break;
            case TrailActivationCondition.VelocityMagnitude:
                break;
        }
    }

    /// <summary>
    /// Trail stop
    /// </summary>
    public void DisableTrail()
    {
        m_WillBeActivatedOnEnable = false;
        m_CanBeAutomaticallyActivated = false;
        DisableTrailEffect();
	}


    /// <summary>
    /// NOT RECOMMANDED : use EnableTrail() instead. Enable the trail effect
    /// </summary>
    /// <param name="forceTrailCreation">Set it to true if you want to force the creation of the trail immediately and spawn the first element</param>
    public void EnableTrailEffect(bool forceTrailCreation = true)
    {
        if (m_SpriteToDuplicate == null)
        {
            Debug.LogAssertion("Warning : trying to EnableTrailEffect while not having a sprite to duplicate set");
            return;
        }

        if (m_CurrentTrailPreset == null)
        {
            Debug.LogAssertion("Warning : trying to EnableTrailEffect while not having a trail preset set");
            return;
        }
        m_TimeTrailStarted = Time.time;
        m_EffectEnabled = true;
        m_CurrentDisplacement = m_ZMoveMax;
        m_PreviousPosSpawned = m_SpriteToDuplicate.transform.position;
        m_PreviousTimeSpawned = Time.time;
        m_PreviousFrameSpawned = Time.frameCount;
        if (forceTrailCreation)
        {
            GenerateNewTrailElement();
        }
    }


    /// <summary>
    /// NOT RECOMMANDED : use DisableTrail() instead. Disable the trail effect
    /// </summary>
    public void DisableTrailEffect()
    {
        m_EffectEnabled = false;
        if(m_HideTrailOnDisabled)
        {
            HideTrail();
        }
    }

    /// <summary>
    /// Hide the current trail, delete all trails elements in this trail.
    /// </summary>
    public void HideTrail()
    {
        while (m_ElementsInTrail.Count > 0)
        {
            TrailElement _tmp = m_ElementsInTrail.Dequeue();
            if (_tmp != null)
                _tmp.Hide();
        }
    }

    /// <summary>
    /// Return true if the trail effect is active
    /// </summary>
    public bool IsEffectEnabled()
    {
        return m_EffectEnabled;
    }
	#endregion
	#region PRIVATE

	void OnLevelWasLoaded()
	{
		TrailElement.ClearFreeElements();
		Destroy(m_TrailElementPrefab);
		m_TrailElementPrefab = null;
	}

	private void Awake()
    {
		if(m_TrailCount == 0)
		{
			TrailElement.ClearFreeElements();
			Destroy(m_TrailElementPrefab);
			m_TrailElementPrefab = null;
		}		

		m_TrailCount++;
		SetTrailParent(m_TrailParent);
        m_CurrentDisplacement = m_ZMoveMax;
        m_TrailElementPrefab = GameObject.Find("TrailElementReference");
        if (m_TrailElementPrefab == null)
        {
            m_TrailElementPrefab = new GameObject();
            m_TrailElementPrefab.AddComponent<SpriteRenderer>();
            m_TrailElementPrefab.AddComponent<TrailElement>();
            m_TrailElementPrefab.name = "TrailElementReference";
#if HIDE_TRAIL_IN_HIERRARCHY
            m_TrailElementPrefab.hideFlags = HideFlags.HideInHierarchy;
#endif
            if (m_TrailParent == null)
            {
                m_TrailElementPrefab.transform.SetParent(m_GlobalTrailContainer, true);
            }
            else
            {
                m_TrailElementPrefab.transform.SetParent(m_LocalTrailContainer, true);
            }
            m_TrailElementPrefab.GetComponent<TrailElement>().Hide(false);
        }

        if (m_SpriteToDuplicate == null)
        {
            m_SpriteToDuplicate = GetComponent<SpriteRenderer>();
        }
        if (m_SpriteToDuplicate == null)
        {
            Debug.LogError("You need a SpriteRenderer on the same GameObject as the SpriteTrail script. Else, you can set the SpriteToDuplicate variable from the inspector");
            return;
        }

        m_PreviousPosSpawned = m_SpriteToDuplicate.transform.position;

        /*if (m_EnabledByDefault)
            EnableTrailEffect();*/
    }

    void CalculateCurrentVelocity()
    {
        if(m_FirstVelocityCheck)
        {
            m_VelocityLocal = ((Vector2)m_SpriteToDuplicate.transform.localPosition - m_PreviousFrameLocalPos) / Time.deltaTime;
            m_VelocityWorld = ((Vector2)m_SpriteToDuplicate.transform.position - m_PreviousFrameWorldPos) / Time.deltaTime;
        }
        m_FirstVelocityCheck = true;
        m_PreviousFrameLocalPos = m_SpriteToDuplicate.transform.localPosition;
        m_PreviousFrameWorldPos = m_SpriteToDuplicate.transform.position;
    }



    void Update()
    {
        if(m_PreviousTrailPreset != m_CurrentTrailPreset)
        {
            if (m_HideTrailOnDisabled)
            {
                HideTrail();
            }
            m_PreviousTrailPreset = m_CurrentTrailPreset;
        }
        if(m_TrailActivationCondition == TrailActivationCondition.VelocityMagnitude || m_TrailDisactivationCondition == TrailDisactivationCondition.VelocityMagnitude)
            CalculateCurrentVelocity();

        if (m_EffectEnabled)
        {
            if (m_CurrentTrailPreset == null)
                return;
            if (m_SpriteToDuplicate == null)
                return;

            switch(m_TrailDisactivationCondition)
            {
                case TrailDisactivationCondition.Manual:
                    break;
                case TrailDisactivationCondition.Time:
                    if(m_TimeTrailStarted + m_TrailActivationDuration <= Time.time)
                    {
                        DisableTrailEffect();
                        return;
                    }
                    break;
                case TrailDisactivationCondition.VelocityMagnitude:
                    float _Velocity = 0;
                    if (m_VelocityStopIsLocalSpace)
                        _Velocity = m_VelocityLocal.magnitude;
                    else
                        _Velocity = m_VelocityWorld.magnitude;

                    if (!m_StopIfOverVelocity && _Velocity >= m_VelocityNeededToStop)
                    {
                        DisableTrailEffect();
                    }
                    else if (m_StopIfOverVelocity && _Velocity <= m_VelocityNeededToStop)
                    {
                        DisableTrailEffect();
                    }
                    break;

            }

            switch (m_CurrentTrailPreset.m_TrailElementSpawnCondition)
            {
                case TrailElementSpawnCondition.Time:
                    if (m_PreviousTimeSpawned + m_CurrentTrailPreset.m_TimeBetweenSpawns <= Time.time)
                    {
                        float _TimeError = Time.time - (m_PreviousTimeSpawned + m_CurrentTrailPreset.m_TimeBetweenSpawns);
                        if (m_CurrentTrailPreset.m_TimeBetweenSpawns > 0)
                        {
                            while (_TimeError >= m_CurrentTrailPreset.m_TimeBetweenSpawns)
                            {
                                _TimeError -= m_CurrentTrailPreset.m_TimeBetweenSpawns;
                            }
                        }
                        if (_TimeError > m_CurrentTrailPreset.m_TimeBetweenSpawns)
                            _TimeError = 0;
                        m_PreviousTimeSpawned = Time.time - _TimeError;

                        GenerateNewTrailElement();
                    }
                    break;
                case TrailElementSpawnCondition.FrameCount:
                    if (m_PreviousFrameSpawned + m_CurrentTrailPreset.m_FramesBetweenSpawns <= Time.frameCount)
                    {
                        m_PreviousFrameSpawned = Time.frameCount;
                        GenerateNewTrailElement();
                    }
                    break;
                case TrailElementSpawnCondition.Distance:
                    if (m_CurrentTrailPreset.m_DistanceCorrection && m_CurrentTrailPreset.m_DistanceBetweenSpawns > 0)
                    {
                        //TODO : if error corrected : generate a timer error ( 3 elements generated at the same frame == 3 elements that update the same)
                        while (Vector2.Distance(m_PreviousPosSpawned, m_SpriteToDuplicate.transform.position) >= m_CurrentTrailPreset.m_DistanceBetweenSpawns)
                        {
                            Vector2 _dir = (Vector2)m_SpriteToDuplicate.transform.position - m_PreviousPosSpawned;
                            Vector2 _tmpPos = m_PreviousPosSpawned + _dir.normalized * m_CurrentTrailPreset.m_DistanceBetweenSpawns;
                            GenerateNewTrailElement(new Vector3( _tmpPos.x, _tmpPos.y, m_SpriteToDuplicate.transform.position.z));
                            m_PreviousPosSpawned = _tmpPos;
                        }
                    }
                    else if (Vector2.Distance(m_PreviousPosSpawned, m_SpriteToDuplicate.transform.position) >= m_CurrentTrailPreset.m_DistanceBetweenSpawns)
                    {
                        GenerateNewTrailElement();
                        m_PreviousPosSpawned = m_SpriteToDuplicate.transform.position;
                    }
                    break;
            }
        }
        else if(m_CanBeAutomaticallyActivated) //check activation condition
        {
            switch(m_TrailActivationCondition)
            {
                case TrailActivationCondition.AlwaysEnabled:
                    EnableTrailEffect(false);
                    break;
                case TrailActivationCondition.Manual:
                    break;
                case TrailActivationCondition.VelocityMagnitude:
                    float _Velocity = 0;
                    if (m_VelocityStopIsLocalSpace)
                        _Velocity = m_VelocityLocal.magnitude;
                    else
                        _Velocity = m_VelocityWorld.magnitude;


                    if (!m_StartIfUnderVelocity && _Velocity >= m_VelocityNeededToStart)
                    {
                        EnableTrailEffect();
                    }
                    else if (m_StartIfUnderVelocity && _Velocity <= m_VelocityNeededToStart)
                    {
                        EnableTrailEffect();
                    }
                    break;
            }
        }

    }

    void GenerateNewTrailElement(Vector3 pos)
    {
        m_CurrentDisplacement -= m_ZMoveStep;
        if(m_CurrentDisplacement <= m_ZMoveStep)
            m_CurrentDisplacement = m_ZMoveMax;
        GameObject _tmpGo = TrailElement.GetFreeElement();
        TrailElement _TmpElement = _tmpGo.GetComponent<TrailElement>();
        //GameObject _tmpGo = GameObject.Instantiate(m_TrailElementPrefab);
        //_tmpGo.name = "TRAIL_" + m_SpriteToDuplicate.name;
        //_tmpGo.hideFlags = HideFlags.None;
        //_tmpGo.transform.Equals(m_SpriteToDuplicate.transform);
        _TmpElement.m_Transform.SetParent(m_SpriteToDuplicate.transform, true);
        _TmpElement.m_Transform.localScale = new Vector3(1, 1, 1);
        _TmpElement.m_Transform.localRotation = Quaternion.identity;
        _TmpElement.m_Transform.localPosition = Vector3.zero;
        if (m_TrailParent == null)
        {
            _TmpElement.m_Transform.SetParent(m_GlobalTrailContainer, true);
        }
        else
        {
            _TmpElement.m_Transform.SetParent(m_LocalTrailContainer, true);
        }
        Vector3 _NewPos = pos;
        _NewPos.z += m_CurrentDisplacement;
        _TmpElement.m_Transform.position = _NewPos;
        _TmpElement.Initialise(this);
        _tmpGo.layer = LayerMask.NameToLayer(m_LayerName);
    }

    void GenerateNewTrailElement()
    {
        m_CurrentDisplacement -= m_ZMoveStep;
        if (m_CurrentDisplacement <= m_ZMoveStep)
            m_CurrentDisplacement = m_ZMoveMax;
        GameObject _tmpGo = TrailElement.GetFreeElement();
        TrailElement _TmpElement = _tmpGo.GetComponent<TrailElement>();
        //GameObject _tmpGo = GameObject.Instantiate(m_TrailElementPrefab);
        //_tmpGo.name = "TRAIL_" + m_SpriteToDuplicate.name;
        //_tmpGo.hideFlags = HideFlags.None;
        //_tmpGo.transform.Equals(m_SpriteToDuplicate.transform);
        _TmpElement.m_Transform.SetParent(m_SpriteToDuplicate.transform, true);
        _TmpElement.m_Transform.localScale = new Vector3(1, 1, 1);
        _TmpElement.m_Transform.localRotation = Quaternion.identity;
        _TmpElement.m_Transform.localPosition = Vector3.zero;
        if (m_TrailParent == null)
        {
            _TmpElement.m_Transform.SetParent(m_GlobalTrailContainer, true);
        }
        else
        {
            _TmpElement.m_Transform.SetParent(m_LocalTrailContainer, true);
        }
        Vector3 _NewPos = _tmpGo.transform.position;
        _NewPos.z = m_CurrentDisplacement;
        _TmpElement.m_Transform.position = _NewPos;
        _TmpElement.Initialise(this);
        _tmpGo.layer = LayerMask.NameToLayer(m_LayerName);
    }

    private void OnDisable()
    {
        DisableTrailEffect();
        switch (m_TrailActivationCondition)
        {
            case TrailActivationCondition.Manual:
                m_WillBeActivatedOnEnable = true;
                break;
        }
    }

    private void OnEnable()
    {
        switch(m_TrailActivationCondition)
        {
            case TrailActivationCondition.Manual:
                if (m_WillBeActivatedOnEnable)
                    EnableTrail();
                break;
            default:
                if (m_CanBeAutomaticallyActivated)
                    EnableTrail();
                break;

        }
        
        
    }

	void OnDestroy()
	{
		m_TrailCount--;
	}

    public static GameObject GetTrailElementPrefab()
    {
        if (m_TrailElementPrefab == null)
        {
            m_TrailElementPrefab = new GameObject();
            m_TrailElementPrefab.AddComponent<SpriteRenderer>();
            m_TrailElementPrefab.AddComponent<TrailElement>();
            m_TrailElementPrefab.name = "TrailElementReference";
#if HIDE_TRAIL_IN_HIERRARCHY
            m_TrailElementPrefab.hideFlags = HideFlags.HideInHierarchy;
#endif
        }
        return m_TrailElementPrefab;
    }
    #endregion
}
