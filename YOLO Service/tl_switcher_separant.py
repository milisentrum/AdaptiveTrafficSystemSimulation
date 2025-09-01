import zmq
import json
import random

def main():
    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind("tcp://*:5556")

    print("Python server started for individual traffic lights control...")

    # Допустим, у нас есть словарь всех светофоров и их состояния
    # место для логики нейронной сети, которая каждую итерацию
    # решает, какой светофор «open»/«close».
    # Ключ: ID светофора, Значение: строка "open" / "close"
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
        # 1) Ждём запрос от Unity (RequestSocket.SendFrame -> сервер .recv_string())
        message = socket.recv_string()
        print(f"Got request from Unity: {message}")

        # 2) Обновляем состояние (именно здесь может быть вызов вашей нейронки)
        # Для демонстрации — случайно "open"/"close"
        # for key in traffic_lights_states:
        #     # traffic_lights_states[key] = random.choice(["open", "close"])
        #     traffic_lights_states[key] = "open"

        # # 3) Формируем список объектов {"id": ..., "state": ...}
        # lights_list = []
        # for light_id, state in traffic_lights_states.items():
        #     lights_list.append({
        #         "id": light_id,
        #         "state": state
        #     })

        lights_list = [
            {"id": lid, "state": st}
            for lid, st in traffic_lights_states.items()
        ]

        # При желании добавляем duration (сколько секунд Unity ждать до следующего запроса)
        # или можно использовать фиксированную паузу 1с/2с на клиенте
        response_dict = {
            "lights": lights_list,
            "duration": 2  # Например, ждать 2 секунды
        }

        response_json = json.dumps(response_dict)
        socket.send_string(response_json)
        print(f"Sent response: {response_json}")

if __name__ == "__main__":
    main()

# import zmq
# import json
# import torch
# import torch.nn as nn

# # Определяем простую MLP — 8 входов → 16 → 8 выходов
# class SimpleTrafficNet(nn.Module):
#     def __init__(self):
#         super(SimpleTrafficNet, self).__init__()
#         self.net = nn.Sequential(
#             nn.Linear(8, 16),
#             nn.ReLU(),
#             nn.Linear(16, 8),
#             nn.Sigmoid()  # чтобы выход был в [0,1]
#         )

#     def forward(self, x):
#         return self.net(x)

# def main():
#     # Инициализируем ZeroMQ
#     context = zmq.Context()
#     socket = context.socket(zmq.REP)
#     socket.bind("tcp://*:5556")
#     print("Python server with neural net started...")

#     # Создаём и инициализируем сеть
#     torch.manual_seed(0)
#     model = SimpleTrafficNet()
#     model.eval()  # режим инференса

#     # Словарь ID светофоров (порядок важен — соответствует входным нейронам)
#     light_ids = [
#         "light_na", "light_ylna", "light_p_na", "light_p_ylna",
#         "light_pol", "light_ylpol", "light_p_pol", "light_p_ylpol"
#     ]

#     while True:
#         # Ждём запрос от Unity
#         _ = socket.recv_string()

#         # 1) Формируем любой вход для сети.
#         #    Здесь просто шум, но можно делать более осмысленные фичи
#         inp = torch.randn(1, len(light_ids))

#         # 2) Прогоняем через сеть
#         with torch.no_grad():
#             out = model(inp).squeeze(0)  # тензор размера [8]

#         # 3) Преобразуем выход в состояния "open"/"close"
#         #    Порог 0.5: >0.5 → open, иначе close
#         states = ["open" if v.item() > 0.5 else "close" for v in out]

#         # 4) Собираем список для JSON
#         lights_list = [
#             {"id": lid, "state": state}
#             for lid, state in zip(light_ids, states)
#         ]

#         # 5) Отправляем ответ
#         response = {
#             "lights": lights_list,
#             "duration": 5  # ждать 2 секунды
#         }
#         socket.send_string(json.dumps(response))
#         print("Sent:", response)

# if __name__ == "__main__":
#     main()
