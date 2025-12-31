extends Node
@onready var tube_server: TubeClient = get_node("../TubeClient")

func _ready():
	tube_server.create_session()
	print("Server session created with ID: %s" % tube_server.session_id)
