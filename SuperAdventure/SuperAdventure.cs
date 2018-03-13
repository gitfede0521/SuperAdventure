using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;

        public SuperAdventure()
        {
            InitializeComponent();

            _player = new Player(10, 10, 20, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
            
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void MoveTo(Location newLocation)
        {
            

            //update the player's current location
            _player.CurrentLocation = newLocation;

            //show/hide available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            //update hit point in ui
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            


            //does the location have any required items    
            if (newLocation.ItemRequiredToEnter !=null)
            {
                if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
                {
                    //we didn't find the required item in their inventory,
                    //so display a message and stop trying to move
                    rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                    return;
                }
            }
            //does the location have a quest?
            if (newLocation.QuestAvailableHere!= null)
            {
                // See if the player already has the quest, and if they've
                //completed it
                bool playerAlreadyHasQuest = 
                    _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = 
                    _player.CompletedThisQuest(newLocation.QuestAvailableHere);
                // See if the player has all the items needed to complete the quest
                bool playerHasAllTheItemsToCompleteQuest =
                    _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);
                //remove quest items from inventory
                _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);
                //add the reward item to the player inventory
                _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);
                //mark quest as completed
                _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
            }

            //does the location have any monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name +
                    Environment.NewLine;

                //make monster
                Monster standardmonster = World.MonsterByID(
                    newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(
                    standardmonster.ID,
                    standardmonster.Name,
                    standardmonster.MaximumDamage,
                    standardmonster.RewardExperiencePoints,
                    standardmonster.RewardGold,
                    standardmonster.CurrentHitPoints,
                    standardmonster.MaximumHitPoints);

                foreach (LootItem lootitem in standardmonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootitem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            //refresh inventory
            UpdateInventoryListInUI();
            //refresh Quest
            UpdateQuestInUI();
            //fresh weapon
            UpdateWeaponListInUI();
            //refresh potions
            UpdatePotionListInUI();

        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem  inventoryitem in _player.Inventory)
            {
                if (inventoryitem.Quantity >0)
                {
                    dgvInventory.Rows.Add(new[]{
                        inventoryitem.Details.Name,
                        inventoryitem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerquest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] {
                    playerquest.Details.Name,
                    playerquest.IsCompleted.ToString() });
            }

        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryitem in _player.Inventory)
            {
                if (inventoryitem.Details is Weapon)
                {
                    if (inventoryitem.Details is Weapon)
                    {
                        if (inventoryitem.Quantity > 0)
                        {
                            weapons.Add((Weapon)inventoryitem.Details);
                        }
                    }
                }
            }

            if (weapons.Count == 0)
            {
                //hide combo box
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryitem in _player.Inventory)
            {
                if (inventoryitem.Details is HealingPotion)
                {
                    if (inventoryitem.Details is HealingPotion)
                    {
                        if (inventoryitem.Quantity > 0)
                        {
                            healingPotions.Add((HealingPotion)inventoryitem.Details);
                        }
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                //hide combo box
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void btnUseWeapon_Click(object sender,EventArgs e)
        {
            Weapon currentweapon = (Weapon)cboWeapons.SelectedItem;
            int damageToMonster = RandomNumberGenerator.NumberBetween(
                currentweapon.MinimumDamage, currentweapon.MaximumDamage);
            _currentMonster.CurrentHitPoints -= damageToMonster;

            rtbMessages.Text += "You hit the " + _currentMonster.Name +
                damageToMonster.ToString() + " points." + Environment.NewLine;

            if (_currentMonster.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + _currentMonster.Name +
                Environment.NewLine;

                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                rtbMessages.Text += "You receive " +
                    _currentMonster.RewardExperiencePoints.ToString() +
                    " experience points" + Environment.NewLine;

                _player.Gold += _currentMonster.RewardGold;
                rtbMessages.Text += "You receive " +
                    _currentMonster.RewardGold.ToString() +
                    " gold" + Environment.NewLine;

                List<InventoryItem> lootedItems = new List<InventoryItem>();

                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1,100)<= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                if (lootedItems.Count==0)
                {
                    foreach (LootItem lootitem in _currentMonster.LootTable)
                    {
                        if (lootitem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootitem.Details, 1));
                        }
                    }
                }

                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    _player.AddItemToInventory(inventoryItem.Details);

                    if (inventoryItem.Quantity==1)
                    {
                        rtbMessages.Text += "You loot " +
                            inventoryItem.Quantity.ToString() + "" +
                            inventoryItem.Details.Name + Environment.NewLine;
                    }
                    else
                    {
                        rtbMessages.Text += "You loot " +
                            inventoryItem.Quantity.ToString() + "" +
                            inventoryItem.Details.NamePlural + Environment.NewLine;
                    }
                }
                //refresh info and inv
                lblHitPoints.Text = _player.CurrentHitPoints.ToString();
                lblGold.Text = _player.Gold.ToString();
                lblExperience.Text = _player.ExperiencePoints.ToString();
                lblLevel.Text = _player.Level.ToString();

                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                rtbMessages.Text += Environment.NewLine;

                MoveTo(_player.CurrentLocation);

            }
            else
            {
                int damageToPlayer =
                    RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                rtbMessages.Text += "The " + _currentMonster.Name + " did " +
                    damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

                _player.CurrentHitPoints -= damageToPlayer;

                lblHitPoints.Text = _player.CurrentHitPoints.ToString();

                if (_player.CurrentHitPoints<=0)
                {
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." +
                        Environment.NewLine;
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.CurrentHitPoints = (_player.CurrentHitPoints + potion.AmountToHeal);

            if (_player.CurrentHitPoints >_player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            foreach (InventoryItem  ii in _player.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;

            int damageToPlayer =
                RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            rtbMessages.Text += "The " + _currentMonster.Name + " did " +
                    damageToPlayer.ToString() + " points of damage." + Environment.NewLine;

            _player.CurrentHitPoints -= damageToPlayer;

            if (_player.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." +
                    Environment.NewLine;
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();


        }

    }
}
