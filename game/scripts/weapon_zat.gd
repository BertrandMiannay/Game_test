extends WeaponBase

# Le Zat'nik'tel a 3 modes de tir :
# Mode 1 → étourdit l'ennemi (ralentit)
# Mode 2 → tue instantanément
# Mode 3 → désintègre le corps (supprime le nœud)
var shot_mode: int = 1

func _ready() -> void:
	weapon_name  = "Zat'nik'tel"
	damage       = 40.0
	fire_rate    = 0.5
	max_ammo     = 20
	reserve_ammo = 60
	reload_time  = 1.2
	is_automatic = false
	super._ready()

func _shoot() -> void:
	can_shoot    = false
	current_ammo -= 1

	ray_cast.force_raycast_update()
	if ray_cast.is_colliding():
		var hit = ray_cast.get_collider()
		if hit:
			match shot_mode:
				1:  # Étourdissement
					if hit.has_method("zat_stun"):
						hit.zat_stun()
					elif hit.has_method("take_damage"):
						hit.take_damage(damage * 0.5)
				2:  # Létal
					if hit.has_method("take_damage"):
						hit.take_damage(9999.0)
				3:  # Désintégration
					if hit.has_method("take_damage"):
						hit.take_damage(9999.0)
					# La désintégration est gérée dans enemy_jaffa via le signal

	# Cycle des modes : 1 → 2 → 3 → 1
	shot_mode = (shot_mode % 3) + 1

	GameManager.ammo_changed.emit(current_ammo, reserve_ammo)
	shoot_timer.start()

func _build_mesh() -> void:
	# Corps du Zat (forme arquée dorée, simulée avec un cylindre courbé)
	var body := CSGCylinder3D.new()
	body.radius  = 0.025
	body.height  = 0.22
	body.rotation.x = PI / 2.0
	body.position = Vector3(0, 0, -0.08)
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.75, 0.60, 0.10)  # Doré Goa'uld
	mat.metallic = 0.8
	mat.roughness = 0.2
	body.material = mat
	add_child(body)

	# Détail avant (tête du Zat)
	var head_mesh := CSGBox3D.new()
	head_mesh.size = Vector3(0.04, 0.04, 0.06)
	head_mesh.position = Vector3(0, 0.01, -0.20)
	var mat2 := StandardMaterial3D.new()
	mat2.albedo_color = Color(0.85, 0.70, 0.20)
	mat2.metallic = 0.9
	head_mesh.material = mat2
	add_child(head_mesh)
