function toProvinceCode(name) {
    if (!name) {
        return name;
    }
    switch (name.trim().toUpperCase()) {
    case "AB":
    case "ALBERTA":
        return "AB";
    case "BC":
    case "BRITISH COLUMBIA":
    case "COLOMBIE-BRITANNIQUE":
        return "BC";
    case "MB":
    case "MANITOBA":
        return "MB";
    case "NB":
    case "NEW BRUNSWICK":
    case "NOUVEAU-BRUNSWICK":
        return "NB";
    case "NL":
    case "TERRE-NEUVE-ET-LABRADOR":
    case "NEWFOUNDLAND AND LABRADOR":
        return "NL";
    case "NT":
    case "NWT":
    case "TNO":
    case "NORD-OUEST":
    case "NORTHWEST TERRITORIES":
    case "TERRITOIRES DU NORD-OUEST":
        return "NT";
    case "NS":
    case "NOVA SCOTIA":
    case "NOUVELLE-ÉCOSSE":
        return "NS";
    case "NU":
    case "NUNAVUT":
    case "NUNAVUT TERRITORY":
        return "NU";
    case "ON":
    case "ONTARIO":
        return "ON";
    case "PE":
    case "PEI":
    case "P.E.I.":
    case "PRINCE EDWARD ISLAND":
    case "ÎLE-DU-PRINCE-ÉDOUARD":
        return "PE";
    case "QC":
    case "QUEBEC":
    case "QUÉBEC":
        return "QC";
    case "SK":
    case "SASKATCHEWAN":
        return "SK";
    case "YT":
    case "YUKON":
    case "THE YUKON":
    case "YUKON TERRITORY":
    case "TERRITOIRE DU YUKON":
        return "YT";
    default:
        return name.toUpperCase();
    }
}