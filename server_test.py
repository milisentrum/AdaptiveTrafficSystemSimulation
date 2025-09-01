import nbformat
import os

# 1) Собираем путь к файлу в текущей директории
notebook_path = os.path.join(os.getcwd(), "phase_analysis.ipynb")

# 2) Создаём новый ноутбук
nb = nbformat.v4.new_notebook()
cells = []

# Заголовок и описание
cells.append(nbformat.v4.new_markdown_cell(
"""# Анализ модели переключения фаз

В этом ноутбуке мы построим:
1. **Кривую обучения** (reward vs episodes)  
2. **Распределение фаз** (как часто модель выбирает каждую)  
3. **Корреляцию** между входными count’ами и фазой  
4. **Динамику** count’ов и фаз во времени  
"""))

# Импорты
cells.append(nbformat.v4.new_code_cell(
"import pandas as pd\nimport matplotlib.pyplot as plt\n%matplotlib inline"
))

# Загрузка лога обучения
cells.append(nbformat.v4.new_code_cell(
"# Лог обучения (VecMonitor)\n"
"train_logs = pd.read_csv('monitor.csv')  # поправьте путь, если файл в другом месте\n"
"train_logs.head()"
))

# Кривая обучения
cells.append(nbformat.v4.new_code_cell(
"plt.figure(figsize=(10,5))\n"
"plt.plot(train_logs['l'].rolling(window=10).mean())\n"
"plt.title('Learning Curve (reward MA10)')\n"
"plt.xlabel('Episode')\nplt.ylabel('Mean Reward')\nplt.show()"
))

# Распределение фаз
cells.append(nbformat.v4.new_code_cell(
"if 'action' in train_logs.columns:\n"
"    train_logs['action'].value_counts().sort_index().plot.pie(autopct='%1.1f%%', figsize=(6,6))\n"
"    plt.title('Action Distribution')\n"
"    plt.ylabel('')\n"
"    plt.show()\n"
"else:\n"
"    print(\"Нет столбца 'action' в monitor.csv\")"
))

# Корреляция
cells.append(nbformat.v4.new_code_cell(
"# Корреляция между входами и фазой\n"
"df = train_logs[['cars_NS','cars_EW','peds_NS','peds_EW','action']]\n"
"corr = df.corr()\n"
"import seaborn as sns\n"
"plt.figure(figsize=(6,5))\n"
"sns.heatmap(corr, annot=True, cmap='coolwarm')\n"
"plt.title('Correlation Matrix')\n"
"plt.show()"
))

# Временные ряды из лога контроллера
cells.append(nbformat.v4.new_code_cell(
"# Динамика во времени\n"
"ctrl = pd.read_csv('controller_logs.csv', parse_dates=['timestamp'])  # поправьте путь\n"
"fig, ax1 = plt.subplots(figsize=(12,6))\n"
"ax1.plot(ctrl['timestamp'], ctrl['cars_NS'], label='cars_NS')\n"
"ax1.plot(ctrl['timestamp'], ctrl['cars_EW'], label='cars_EW')\n"
"ax1.plot(ctrl['timestamp'], ctrl['peds_NS'], label='peds_NS')\n"
"ax1.plot(ctrl['timestamp'], ctrl['peds_EW'], label='peds_EW')\n"
"ax1.set_ylabel('Count')\n"
"ax1.legend(loc='upper left')\n\n"
"ax2 = ax1.twinx()\n"
"ax2.plot(ctrl['timestamp'], ctrl['phase'], '--k', label='phase')\n"
"ax2.set_ylabel('Phase')\n"
"ax2.legend(loc='upper right')\n\n"
"plt.title('Dynamics of counts and phase over time')\n"
"plt.show()"
))

# Записываем ноутбук
nb['cells'] = cells
with open(notebook_path, 'w', encoding='utf-8') as f:
    nbformat.write(nb, f)

print(f"Ноутбук с графиками создан: {notebook_path}")
