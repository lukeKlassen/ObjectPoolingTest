using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject m_PooledGameObject = null; // game object to pool 
    [SerializeField] private int m_NumPooled = 10; // number of game objects to store in pool
    [SerializeField] private string m_PoolStats;
    private List<GameObject> m_InactivePool; // list of gameobjects which can be reused
    private List<GameObject> m_ActivePool; // list of gameobjects which are in use

    void Start()
    {
        // print errors on incorrect usage
        if ( m_PooledGameObject == null ) {
            Debug.LogError( "Pooling failed - No game object found to pool" );
            return;
        }
        if ( m_NumPooled <= 0 ) {
            Debug.LogError( "Pooling failed - number of objects to pool must be greater than 0 " );
            return;
        }
        m_InactivePool = new List<GameObject>( m_NumPooled );
        m_ActivePool = new List<GameObject>( m_NumPooled );
        // instantiate game objects up to num pooled, adding them to the inactive pool
        for ( int i = m_InactivePool.Count; i < m_NumPooled; ++i ) {
            GameObject g = Instantiate( m_PooledGameObject );
            g.SetActive( false );
            m_InactivePool.Add( g );
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        m_PoolStats = "num Pooled: " + m_NumPooled + " inactive: " + m_InactivePool.Count + " active: " + m_ActivePool.Count;
#endif
    }

    /*
        Returns an object from the inactive pool of objects
        If all objects in the pool are in use, it will instead Instantiate and return a new game object.
        Optimized with two pools to return new objects efficiently, moving cost over to deactivation
    */
    public GameObject GetInactivePoolObject() {
        while ( m_InactivePool.Count > 0 ) { // for safety we run through inactive pool, checking to make sure our object is inactive
            GameObject poppedObject = m_InactivePool[ m_InactivePool.Count - 1 ];
            m_InactivePool.RemoveAt(m_InactivePool.Count - 1); // RemoveAt is O(1) at last index
            m_ActivePool.Add( poppedObject );
            if ( poppedObject.activeInHierarchy ) { // if we find an active object in the inactive pool, swap it immediately and don't return it
                Debug.LogWarning( "Active PoolableObject found in inactive pool" );
            } else {
                return poppedObject;
            }
        }
        // no objects in inactive pool found, Instantiate another object
        Debug.LogWarning( "Ran out of pooled objects, had to instantiate new object" );
        GameObject g = Instantiate( m_PooledGameObject );
        m_ActivePool.Add( g );
        m_NumPooled++;
        return g;
    }

    /*
        Deactivates the given object by setting it to be inactive and moving it to the 
        inactive pool for future use.
    */
    public void DeactivatePoolObject( GameObject g ) {
        g.SetActive( false );
        m_ActivePool.Remove( g ); // requires linear search through the active pool
        if ( !m_InactivePool.Contains(g) ) { // requires linear search through inactive pool
            m_InactivePool.Add( g );
        }
    }

    /*
        Waits for 'timeDelay' seconds before calling 'DeactivatePoolObject' on the input game object
    */
    public void DelayDeactivate( GameObject g, float timeDelay ) {
        StartCoroutine( DelayCoroutine( g, timeDelay ) );
    }

    private IEnumerator DelayCoroutine( GameObject g, float timeDelay ) {
        yield return new WaitForSeconds( timeDelay ); // wait timeDelay before deactivating the pool object
        DeactivatePoolObject( g );
    }
}