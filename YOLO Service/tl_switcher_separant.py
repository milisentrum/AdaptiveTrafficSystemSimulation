import zmq
import json
import random

def main():
    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind("tcp://*:5556")

    print("Python server started for individual traffic lights control...")
    
    traffic_lights_states = {
        "light_na":     "close",
        "light_ylna":   "close",
        "light_p_na":   "open",
        "light_p_ylna": "close",
        "light_pol":    "close",
        "light_ylpol":  "close",
        "light_p_pol":  "close",
        "light_p_ylpol":"close"
    }

    while True:
        message = socket.recv_string()
        print(f"Got request from Unity: {message}")


        lights_list = [
            {"id": lid, "state": st}
            for lid, st in traffic_lights_states.items()
        ]

        response_dict = {
            "lights": lights_list,
            "duration": 2  # ждать 2 секунды
        }

        response_json = json.dumps(response_dict)
        socket.send_string(response_json)
        print(f"Sent response: {response_json}")

if __name__ == "__main__":
    main()
