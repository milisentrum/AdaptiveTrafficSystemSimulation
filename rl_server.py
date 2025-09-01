import zmq
import json
import numpy as np
from stable_baselines3 import PPO

# Загрузка модели
model = PPO.load("ppo_cross_final.zip", device="cpu")
model.policy.eval()

# Порядок групп для наблюдения
GROUPS = ["cars_NS", "cars_EW", "peds_NS", "peds_EW"]

def make_obs(counts: dict) -> np.ndarray:
    # добавляем два нуля для wait_ns и wait_ew
    return np.array([counts[g] for g in GROUPS] + [0, 0], dtype=np.float32)

# Фазы: пример соответствия action → список открытых IDs
phase_map = {
    0: ["light_na",    "light_p_na"],
    1: ["light_pol",   "light_p_pol"],
    2: ["light_ylna",  "light_p_ylna"],
    3: ["light_ylpol", "light_p_ylpol"]
}

# Полный список всех светофоров
all_lights = [
    "light_na","light_ylna","light_p_na","light_p_ylna",
    "light_pol","light_ylpol","light_p_pol","light_p_ylpol"
]

def main():
    ctx = zmq.Context()
    sock = ctx.socket(zmq.REP)
    sock.bind("tcp://*:5556")
    print("Controller started on port 5556")

    while True:
        msg = sock.recv_string()
        print("── raw payload:", msg)
        try:
            data = json.loads(msg)
            counts = data["counts"]
            # печатаем только после успешного разбора
            print("→ parsed counts:", counts)
        except Exception as e:
            print("❌ JSON parse error:", e)
            sock.send_string(json.dumps({"error":"bad json"}))
            continue   # пропускаем оставшуюся логику и ждём следующий запрос

        # Диагностика входных счётчиков
        print("📥 Received counts:", counts)

        obs = make_obs(counts)
        action, _ = model.predict(obs, deterministic=True)
        phase = int(action)

        # 1) базовый маппинг
        open_ids = set(phase_map[phase])

        # 2) «раскрытие» пары жёлтый + противоположный пешеходный
        if "light_na" in open_ids:
            open_ids.update(["light_ylna", "light_p_pol", "light_p_ylpol"])
        elif "light_pol" in open_ids:
            open_ids.update(["light_ylpol", "light_p_na", "light_p_ylna"])

        # 3) динамические единичные открытия по порогам
        if counts["peds_NS"] > 7:
            open_ids.update(["light_p_na", "light_p_ylna"])
        if counts["peds_EW"] > 7:
            open_ids.update(["light_p_pol", "light_p_ylpol"])
        if counts["cars_NS"] > 5:
            open_ids.update(["light_na", "light_ylna"])
        if counts["cars_EW"] > 5:
            open_ids.update(["light_pol", "light_ylpol"])

        # 4) формируем финальный ответ
        response_lights = []
        for lid in all_lights:
            state = "open" if lid in open_ids else "close"
            response_lights.append({"id": lid, "state": state})


        # Отладочный вывод по каждому светофору
        print(f"🔄 Chosen phase: {phase}")
        for ld in response_lights:
            print(f"    {ld['id']}: {ld['state']}")

        # Отправка ответа
        # пример: если мало машин — быстрее, много — дольше
        total_cars = counts["cars_NS"] + counts["cars_EW"]
        # dur = max(1.0, min(5.0, 10.0 / (1 + total_cars)))
        dur = 7.0
        resp = {"lights": response_lights, "duration": dur}
        sock.send_string(json.dumps(resp))

if __name__ == "__main__":
    main()
