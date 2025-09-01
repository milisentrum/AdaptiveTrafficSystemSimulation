import zmq, json
import numpy as np
from stable_baselines3 import PPO

# 1) Загружаем обученную модель
model = PPO.load("ppo_cross_final")  # без .zip в названии

# 2) Определяем соответствие action → открытые фазы
#    Смотри roadnet.json: фазы 0–3 (две автомобильные, две пешеходные)
PHASES = [
    ["light_na", "light_ylna"],       # action == 0 → открыть авто NS
    ["light_pol", "light_ylpol"],     # action == 1 → открыть авто EW
    ["light_p_na", "light_p_ylna"],   # action == 2 → открыть пешеходы NS
    ["light_p_pol", "light_p_ylpol"], # action == 3 → открыть пешеходы EW
]

# полный список всех 8 ID, чтобы остальные закрыть
ALL_LIGHT_IDS = [
    "light_na", "light_ylna", "light_p_na", "light_p_ylna",
    "light_pol", "light_ylpol", "light_p_pol", "light_p_ylpol"
]

def make_response(action: int, duration: float = 5):
    """По номеру фазы строим JSON c open/close для каждого света."""
    on_ids = set(PHASES[action])
    lights = []
    for lid in ALL_LIGHT_IDS:
        lights.append({
            "id": lid,
            "state": "open" if lid in on_ids else "close"
        })
    return {"lights": lights, "duration": duration}

def main():
    ctx = zmq.Context()
    sock = ctx.socket(zmq.REP)
    sock.bind("tcp://*:5556")
    print("[TL_SWITCHER] RL-сервер запущен на 5556, ждём obs…")

    while True:
        # 3) Получаем от Unity аггрегированный obs-JSON
        #    {"cars_ns":…, "cars_ew":…, "peds_ns":…, "peds_ew":…, "wait_ns":…, "wait_ew":…}
        msg = sock.recv_string()
        data = json.loads(msg)

        # 4) Формируем вектор obs в том порядке, который ждал агент
        obs = np.array([
            data["cars_ns"],
            data["cars_ew"],
            data["peds_ns"],
            data["peds_ew"],
            data["wait_ns"],
            data["wait_ew"],
        ], dtype=np.float32)

        # 5) Получаем действие от PPO
        action, _ = model.predict(obs, deterministic=True)

        # 6) Строим ответ и шлём обратно
        resp = make_response(int(action), duration=5)
        sock.send_string(json.dumps(resp))

        print(f"[TL_SWITCHER] obs={obs.tolist()} → action={action} → resp sent")

if __name__ == "__main__":
    main()
