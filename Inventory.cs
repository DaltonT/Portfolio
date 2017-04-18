using UnityEngine;
using System.Collections;

public class Inventory : MonoBehaviour {

	//List of objects in inventory
	public GameObject[] inventory;
	PlayerInput pI;
	//-1 = no item equipped
	public int activeSlot = -1;
	Transform hand_Right;

	void Start () {
		inventory = new GameObject[10];
		pI = GetComponentInParent<PlayerInput> ();
		hand_Right = transform.parent.Find ("Render_Main/Hand_Right");
	}
	
	// Update is called once per frame
	void Update () {
		if (pI.numDown != -1) {
			EquipItem (pI.numDown);
		}
	}

	public void EquipItem (int slot){
		if (inventory [slot] == null)
			return;
		
		if (activeSlot != -1 && inventory [activeSlot] != null) {		
			inventory [activeSlot].transform.parent = null;
			inventory [activeSlot].SetActive (false);
		}

		//If equipping already equipped item, unequip.  Else, equip item.
		activeSlot = (slot == activeSlot ? -1 : slot);

		if (activeSlot!=-1) {
			MoveEquippedToHand();
		}
	}

	//Set equipped object to active and move it into right hand position.
	void MoveEquippedToHand(){
		inventory [activeSlot].SetActive (true);
		inventory [activeSlot].transform.parent = hand_Right;
		inventory [activeSlot].transform.localPosition = Vector3.zero;
		inventory [activeSlot].transform.localRotation = Quaternion.Euler (Vector3.zero);
	}

	public void AddItem (GameObject item){
		for (int i = 1; i < inventory.Length+1; i++) {
			if (i == inventory.Length)
				i = 0;
			if (inventory [i] == null) {
				item.GetComponent<PickupItem> ().Pickup(transform.parent.gameObject);
				inventory [i] = item;
				item.SetActive (false);
				return;
			}
			if (i == 0)
				i = inventory.Length+1;
		}
	}

	public void DropItem (){
		MoveEquippedToHand();
		inventory [activeSlot].transform.parent = null;
		inventory [activeSlot].GetComponent<PickupItem> ().Drop ();
		inventory [activeSlot] = null;
		activeSlot = -1;
	}

	public void DropAllItems(){
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory [i] != null) {
				activeSlot = i;
				DropItem ();
			}
		}
	}
}
