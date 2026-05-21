static class L
{
    public static string Current = "en";

    public static readonly Dictionary<string, (string Flag, string Name)> Languages = new()
    {
        ["en"] = ("🇬🇧", "English"),
        ["de"] = ("🇩🇪", "Deutsch"),
        ["fr"] = ("🇫🇷", "Français"),
        ["es"] = ("🇪🇸", "Español"),
        ["it"] = ("🇮🇹", "Italiano"),
    };

    public static readonly string[] Codes = [.. Languages.Keys];

    private static readonly Dictionary<string, Dictionary<string, string>> _t = new()
    {
        ["en"] = new()
        {
            ["title"]       = "Wallpaper Picker",
            ["loadBtn"]     = "Load new images",
            ["setBtn"]      = "Set as wallpaper",
            ["favBtn"]      = "★ Favorites",
            ["favTitle"]    = "Favorites",
            ["favEmpty"]    = "No favorites yet. Star an image to save it here.",
            ["favRemove"]   = "✕ Remove",
            ["ready"]       = "Click 'Load new images' to get started.",
            ["loading"]     = "Loading {0} images…",
            ["loaded"]      = "Images loaded. Pick one.",
            ["selected"]    = "Image selected. Ready to set.",
            ["downloading"] = "Downloading high-res image (2880×1800)…",
            ["setting"]     = "Setting wallpaper…",
            ["done"]        = "Done! Wallpaper updated.",
            ["errorLoad"]   = "Error loading: {0}",
            ["errorSet"]    = "Error: {0}",
        },
        ["de"] = new()
        {
            ["title"]       = "Hintergrundbild-Picker",
            ["loadBtn"]     = "Neue Bilder laden",
            ["setBtn"]      = "Als Hintergrund setzen",
            ["favBtn"]      = "★ Favoriten",
            ["favTitle"]    = "Favoriten",
            ["favEmpty"]    = "Noch keine Favoriten. Markiere ein Bild mit dem Stern.",
            ["favRemove"]   = "✕ Entfernen",
            ["ready"]       = "Klicke auf 'Neue Bilder laden', um zu starten.",
            ["loading"]     = "Lade {0} neue Bilder herunter…",
            ["loaded"]      = "Bilder geladen. Wähle eins aus.",
            ["selected"]    = "Bild ausgewählt. Bereit zum Setzen.",
            ["downloading"] = "Lade High-Res Bild (2880×1800) herunter…",
            ["setting"]     = "Setze Hintergrund…",
            ["done"]        = "Erfolgreich! Hintergrund wurde aktualisiert.",
            ["errorLoad"]   = "Fehler beim Laden: {0}",
            ["errorSet"]    = "Fehler: {0}",
        },
        ["fr"] = new()
        {
            ["title"]       = "Sélecteur de fond d'écran",
            ["loadBtn"]     = "Charger de nouvelles images",
            ["setBtn"]      = "Définir comme fond d'écran",
            ["favBtn"]      = "★ Favoris",
            ["favTitle"]    = "Favoris",
            ["favEmpty"]    = "Aucun favori. Étoilez une image pour la sauvegarder.",
            ["favRemove"]   = "✕ Supprimer",
            ["ready"]       = "Cliquez sur 'Charger de nouvelles images' pour commencer.",
            ["loading"]     = "Chargement de {0} images…",
            ["loaded"]      = "Images chargées. Choisissez-en une.",
            ["selected"]    = "Image sélectionnée. Prête à appliquer.",
            ["downloading"] = "Téléchargement haute résolution (2880×1800)…",
            ["setting"]     = "Application du fond d'écran…",
            ["done"]        = "Terminé ! Fond d'écran mis à jour.",
            ["errorLoad"]   = "Erreur de chargement : {0}",
            ["errorSet"]    = "Erreur : {0}",
        },
        ["es"] = new()
        {
            ["title"]       = "Selector de fondo de pantalla",
            ["loadBtn"]     = "Cargar nuevas imágenes",
            ["setBtn"]      = "Establecer como fondo",
            ["favBtn"]      = "★ Favoritos",
            ["favTitle"]    = "Favoritos",
            ["favEmpty"]    = "Sin favoritos. Marca una imagen con la estrella.",
            ["favRemove"]   = "✕ Eliminar",
            ["ready"]       = "Haz clic en 'Cargar nuevas imágenes' para comenzar.",
            ["loading"]     = "Cargando {0} imágenes…",
            ["loaded"]      = "Imágenes cargadas. Elige una.",
            ["selected"]    = "Imagen seleccionada. Lista para aplicar.",
            ["downloading"] = "Descargando imagen en alta resolución (2880×1800)…",
            ["setting"]     = "Aplicando fondo de pantalla…",
            ["done"]        = "¡Listo! Fondo de pantalla actualizado.",
            ["errorLoad"]   = "Error al cargar: {0}",
            ["errorSet"]    = "Error: {0}",
        },
        ["it"] = new()
        {
            ["title"]       = "Selettore sfondi",
            ["loadBtn"]     = "Carica nuove immagini",
            ["setBtn"]      = "Imposta come sfondo",
            ["favBtn"]      = "★ Preferiti",
            ["favTitle"]    = "Preferiti",
            ["favEmpty"]    = "Nessun preferito. Aggiungi una stella a un'immagine.",
            ["favRemove"]   = "✕ Rimuovi",
            ["ready"]       = "Clicca su 'Carica nuove immagini' per iniziare.",
            ["loading"]     = "Caricamento di {0} immagini…",
            ["loaded"]      = "Immagini caricate. Scegline una.",
            ["selected"]    = "Immagine selezionata. Pronta per l'impostazione.",
            ["downloading"] = "Download alta risoluzione (2880×1800)…",
            ["setting"]     = "Impostazione dello sfondo…",
            ["done"]        = "Fatto! Sfondo aggiornato.",
            ["errorLoad"]   = "Errore durante il caricamento: {0}",
            ["errorSet"]    = "Errore: {0}",
        },
    };

    public static string Get(string key) =>
        _t.TryGetValue(Current, out var d) && d.TryGetValue(key, out var v) ? v : key;

    public static string Status(string key, string arg = "") =>
        string.Format(Get(key), arg);
}
