extends Node

# ─── Signaux globaux ───────────────────────────────────────────────────────────
signal health_changed(current: float, maximum: float)
signal ammo_changed(current: int, reserve: int)
signal weapon_changed(weapon: Node)
signal enemy_killed(remaining: int)
signal game_over
signal victory

# ─── État de la partie ─────────────────────────────────────────────────────────
var enemies_remaining: int = 0
var game_ended: bool = false

# ─── Cycle de vie ──────────────────────────────────────────────────────────────
func _ready() -> void:
	_setup_input_actions()

# ─── Gestion des ennemis ───────────────────────────────────────────────────────
func register_enemy() -> void:
	enemies_remaining += 1

func notify_enemy_killed() -> void:
	enemies_remaining = max(0, enemies_remaining - 1)
	enemy_killed.emit(enemies_remaining)

# ─── Fin de partie ─────────────────────────────────────────────────────────────
func player_died() -> void:
	if game_ended:
		return
	game_ended = true
	game_over.emit()

func player_won() -> void:
	if game_ended:
		return
	game_ended = true
	victory.emit()

func restart() -> void:
	game_ended = false
	enemies_remaining = 0
	get_tree().reload_current_scene()

# ─── Définition des touches (programmatique) ──────────────────────────────────
func _setup_input_actions() -> void:
	var actions := {
		"move_forward":  [KEY_W, KEY_UP],
		"move_back":     [KEY_S, KEY_DOWN],
		"move_left":     [KEY_A, KEY_LEFT],
		"move_right":    [KEY_D, KEY_RIGHT],
		"jump":          [KEY_SPACE],
		"sprint":        [KEY_SHIFT],
		"reload":        [KEY_R],
		"weapon_1":      [KEY_1],
		"weapon_2":      [KEY_2],
		"restart":       [KEY_ENTER],
	}

	for action_name in actions:
		if not InputMap.has_action(action_name):
			InputMap.add_action(action_name)
		for keycode in actions[action_name]:
			var ev := InputEventKey.new()
			ev.physical_keycode = keycode
			InputMap.action_add_event(action_name, ev)

	# Tir = clic gauche
	if not InputMap.has_action("shoot"):
		InputMap.add_action("shoot")
	var mouse_ev := InputEventMouseButton.new()
	mouse_ev.button_index = MOUSE_BUTTON_LEFT
	InputMap.action_add_event("shoot", mouse_ev)
