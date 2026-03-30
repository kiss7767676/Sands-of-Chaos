using UnityEngine;

public class Interactable : MonoBehaviour
{

    protected PlayerWeaponController weaponController;


    [SerializeField] protected MeshRenderer mesh;

    [SerializeField] private Material highlightMaterial;
    [SerializeField] protected Material defaultMaterial;

    private void Start()
    {
        if (mesh == null)
            mesh = GetComponentInChildren<MeshRenderer>();

        if (mesh == null)// remove if error occurs later
        {
            return;
        }

        defaultMaterial = mesh.sharedMaterial;
    }

    protected void UpdateMeshAndMaterial(MeshRenderer newMesh)
    {
        mesh = newMesh;
        defaultMaterial = newMesh.sharedMaterial;
    }

    public virtual void Interaction()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }

    public void HighlightActive(bool active)
    {
        if (active)
        {
            mesh.material = highlightMaterial;
        }
        else
        {
            mesh.material = defaultMaterial;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {

        if (weaponController == null)
        {
            weaponController = other.GetComponent<PlayerWeaponController>();
        }

        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        if (playerInteraction == null)
            return;

        //HighlightActive(true);

        playerInteraction.GetInteractables().Add(this);

        playerInteraction.UpdateClosestInteractable();

    }

    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();

        if (playerInteraction == null)
            return;

        HighlightActive(false);

        playerInteraction.GetInteractables().Remove(this);
        playerInteraction.UpdateClosestInteractable();
    }
}
