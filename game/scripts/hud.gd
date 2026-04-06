extends CanvasLayer

@onready var health_bar:    ProgressBar = $Control/HealthBar
@onready var health_label:  Label       = $Control/HealthLabel
@onready var ammo_label:    Label       = $Control/AmmoLabel
@onready var weapon_label:  Label       = $Control/WeaponLabel
@onready var message_label: Label       = $Control/MessageLabel
@onready var enemies_label: Label       = $Control/EnemiesLabel

func _ready() -> void:
	GameManager.health_changed.connect(_on_health_changed)
	GameManager.ammo_changed.connect(_on_ammo_changed)
	GameManager.weapon_changed.connect(_on_weapon_changed)
	GameManager.enemy_killed.connect(_on_enemy_killed)
	GameManager.game_over.connect(_on_game_over)
	GameManager.victory.connect(_on_victory)

	health_bar.value   = 100
	health_label.text  = "100 / 100"
	message_label.text = ""
	_update_enemies_label()

func _update_enemies_label() -> void:
	enemies_label.text = "Jaffa : %d" % GameManager.enemies_remaining

func _on_health_changed(current: float, maximum: float) -> void:
	health_bar.value  = (current / maximum) * 100.0
	health_label.text = "%d / %d" % [int(current), int(maximum)]

func _on_ammo_changed(current: int, reserve: int) -> void:
	ammo_label.text = "%d  |  %d" % [current, reserve]

func _on_weapon_changed(weapon: Node) -> void:
	weapon_label.text = weapon.weapon_name
	ammo_label.text   = "%d  |  %d" % [weapon.current_ammo, weapon.reserve_ammo]

func _on_enemy_killed(_remaining: int) -> void:
	_update_enemies_label()
	if GameManager.enemies_remaining == 0:
		_show_message("Tous les Jaffa éliminés !\nRejoignez la Porte des Étoiles !", Color.YELLOW)

func _on_game_over() -> void:
	_show_message("MISSION ÉCHOUÉE\n\n[Entrée] Recommencer", Color.RED)

func _on_victory() -> void:
	_show_message("MISSION ACCOMPLIE\n\nVous avez traversé la Porte !", Color(1.0, 0.85, 0.0))

func _show_message(text: String, color: Color) -> void:
	message_label.text = text
	message_label.add_theme_color_override("font_color", color)
