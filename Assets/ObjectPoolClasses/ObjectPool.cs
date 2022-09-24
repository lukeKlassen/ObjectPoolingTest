using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject m_pooledGameObject = null; // game object to pool 
    [SerializeField] private int m_numPooled = 10; // number of game objects to store in pool
    [SerializeField] private string m_poolStats;
    private List<GameObject> _inactivePool; // list of gameobjects which can be reused
    private List<GameObject> _activePool; // list of gameobjects which are in use

    void Start()
    {
        // print errors on incorrect usage
        if ( m_pooledGameObject == null ) {
            Debug.LogError( "Pooling failed - No game object found to pool" );
            return;
        }
        if ( m_numPooled <= 0 ) {
            Debug.LogError( "Pooling failed - number of objects to pool must be greater than 0 " );
            return;
        }
        _inactivePool = new List<GameObject>( m_numPooled );
        _activePool = new List<GameObject>( m_numPooled );
        // instantiate game objects up to num pooled, adding them to the inactive pool
        for ( int i = 0; i < m_numPooled; ++i ) {
            GameObject g = Instantiate( m_pooledGameObject );
            g.SetActive(false);
            _inactivePool.Add( g );
        }
    }

    void Update()
    {
        m_poolStats = "num Pooled: " + m_numPooled + " inactive: " + _inactivePool.Count + " active: " + _activePool.Count;
    }

    /*
        Returns an object from the pool which has inUse: False
        If all objects in the pool are in use, it will instead Instantiate and return 
        a new game object of this type. 
    */
    public GameObject GetInactivePoolObject() {
        while ( _inactivePool.Count > 0 ) { // for safety we run through inactive pool, checking to make sure our object is inactive
            GameObject poppedObject = _inactivePool[ _inactivePool.Count - 1 ];
            _inactivePool.RemoveAt(_inactivePool.Count - 1); // RemoveAt is O(1) at last index
            _activePool.Add( poppedObject );
            if ( poppedObject.activeSelf ) { // if we find an active object in the inactive pool, swap it immediately and don't return it
                Debug.LogError( "Active PoolableObject found in inactive pool" );
            } else {
                return poppedObject;
            }
        }
        // no objects in inactive pool found, Instantiate another object
        GameObject g = Instantiate( m_pooledGameObject );
        _activePool.Add( g );
        m_numPooled++;
        return g;
    }

    /*
        Deactivates the given object by setting IsActive: False and marks it for future reuse
    */
    public void DeactivatePoolObject( GameObject g ) {
        g.SetActive( false );
        _activePool.Remove( g ); // requires linear search through the active pool
        _inactivePool.Add( g );
    }

    public void DelayDeactivate( GameObject g, float timeDelay ) {
        StartCoroutine( DelayCoroutine( g, timeDelay ) );
    }

    private IEnumerator DelayCoroutine( GameObject g, float timeDelay ) {
        yield return new WaitForSeconds( timeDelay ); // wait timeDelay before deactivating the pool object
        DeactivatePoolObject( g );
    }
}