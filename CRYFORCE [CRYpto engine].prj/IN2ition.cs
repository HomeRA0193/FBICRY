﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRYFORCE.Engine
{
	/// <summary>
	/// Класс, реализующий интерактивную текстовую стеганографию
	/// </summary>
	/// <remarks>Это новый тип стеганографии (по-сути это общий случай синонимного метода, но более простой и мощный),
	/// требующий от оператора непосредственного участия. Текст, несущий данные, должен формироваться со значительной
	/// смысловой избыточностью на уровне предложений. На его основе будет сформирован словарь, в котором каждому слову
	/// будет сопоставлена битовая нагрузка "0" или "1". Для организации уникального способа нагрузки, к каждому слову
	/// предварительно будет добавляться модификатор. Все слова обрабатываются без учета регистра и знаков препинания,
	/// короткие слова (короче 4 символов) не учитываются (однако все игнорируемые символы и слова тем не менее попадают
	/// в целевой поток, в том числе сохраняется регистр). Если слово не соответствует битовой нагрузке и должно быть
	/// заменено другим, оно выделяется заглавными буквами (в опциональном поле текстового вывода).</remarks>
	class IN2ition
	{
		#region Static
		#endregion Static

		#region Constants
		#endregion Constants

		#region Data
		#endregion Data

		#region Events
		#endregion Events

		#region .ctor
		#endregion .ctor

		#region Properties
		#endregion Properties

		#region Private
		#endregion Private

		#region Protected
		#endregion Protected

		#region Public
		#endregion Public
	}
}
