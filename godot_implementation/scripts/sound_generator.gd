class_name SoundGenerator

const SAMPLE_RATE := 44100
const SAMPLE_TIME := 1.0 / SAMPLE_RATE

# ─── P90 : bruit blanc court + thump basse fréquence ─────────────────────────
static func make_p90() -> AudioStreamWAV:
	var duration := 0.07
	var n := int(SAMPLE_RATE * duration)
	var data := PackedByteArray()
	data.resize(n * 2)
	for i in n:
		var t    := float(i) * SAMPLE_TIME
		var env  := exp(-t * 25.0)
		var noise := randf_range(-1.0, 1.0)
		var thump := sin(TAU * 32.0 * t) * 0.85
		var sub   := sin(TAU * 18.0 * t) * 0.4
		var val  := clampf((noise * 0.2 + thump + sub) * env, -1.0, 1.0)
		var s    := int(val * 32767)
		data[i * 2]     = s & 0xFF
		data[i * 2 + 1] = (s >> 8) & 0xFF
	return _build(data)

# ─── Zat'nik'tel : descente de fréquence (énergie Goa'uld) ───────────────────
static func make_zat() -> AudioStreamWAV:
	var duration := 0.24
	var n := int(SAMPLE_RATE * duration)
	var data := PackedByteArray()
	data.resize(n * 2)
	for i in n:
		var t   := float(i) * SAMPLE_TIME
		var env := exp(-t * 9.0)
		# Sweep de 700 Hz → 180 Hz
		var phase := TAU * (700.0 * t - 260.0 * t * t / duration)
		# Harmonique légère pour texture
		var harmonic := sin(phase * 2.0) * 0.15
		var val := clampf((sin(phase) * 0.85 + harmonic) * env, -1.0, 1.0)
		var s   := int(val * 32767)
		data[i * 2]     = s & 0xFF
		data[i * 2 + 1] = (s >> 8) & 0xFF
	return _build(data)

# ─── Bâton de combat Goa'uld : décharge plasma grave ─────────────────────────
static func make_staff() -> AudioStreamWAV:
	var duration := 0.30
	var n := int(SAMPLE_RATE * duration)
	var data := PackedByteArray()
	data.resize(n * 2)
	for i in n:
		var t   := float(i) * SAMPLE_TIME
		var env := exp(-t * 12.0)
		# Sweep descendant 220 Hz → 60 Hz
		var phase := TAU * (220.0 * t - 80.0 * t * t / duration)
		var body  := sin(phase) * 0.75
		# Harmonique haute pour la texture plasma
		var crack := sin(phase * 3.0) * 0.2 * exp(-t * 30.0)
		var val   := clampf((body + crack) * env, -1.0, 1.0)
		var s     := int(val * 32767)
		data[i * 2]     = s & 0xFF
		data[i * 2 + 1] = (s >> 8) & 0xFF
	return _build(data)

# ─── Utilitaire ───────────────────────────────────────────────────────────────
static func _build(data: PackedByteArray) -> AudioStreamWAV:
	var stream := AudioStreamWAV.new()
	stream.data     = data
	stream.format   = AudioStreamWAV.FORMAT_16_BITS
	stream.mix_rate = SAMPLE_RATE
	stream.stereo   = false
	return stream
