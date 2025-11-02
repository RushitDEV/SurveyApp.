namespace SurveyApp.Models
{
    public enum QuestionType
    {
        SingleChoice = 1,       // Tek Seçmeli
        MultipleChoice = 2,     // Çok Seçmeli
        OpenEnded = 3,          // Açýk Uçlu (Uzun Metin)
        ShortText = 4,          // Kýsa Metin
        ImageUpload = 5,        // Resim Eklemeli
        FileUpload = 6,         // Dosya Yükleme
        DatePicker = 7,         // Tarih Seçimi
        Number = 8,             // Sayýsal Giriþ
        Rating5 = 9,            // Derecelendirme (1-5)
        Rating10 = 10           // Derecelendirme (1-10)
    }
}