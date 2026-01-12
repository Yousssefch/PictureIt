extends Node
@onready var tube_client: TubeClient = get_node("../TubeClient")
@export var session_id = ""

func create_session():
	tube_client.create_session()
	session_id = tube_client.session_id


func join_session(id):
	tube_client.join_session(id)
	session_id = id

func leave_session():
	tube_client.leave_session()
