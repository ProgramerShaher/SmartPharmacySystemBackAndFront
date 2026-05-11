import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, Alert, ActivityIndicator } from 'react-native';
import { useAuth } from '../context/AuthContext';
import { AuthService } from '../services/authService';

const LoginScreen = ({ navigation }: any) => {
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();

  const handleLogin = async () => {
    if (!phone || !password) {
      Alert.alert('خطأ', 'يرجى إدخال رقم الهاتف وكلمة المرور');
      return;
    }

    setLoading(true);
    try {
      const result = await AuthService.login({ phoneNumber: phone, password });
      login(result);
    } catch (e: any) {
      Alert.alert('فشل تسجيل الدخول', e.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={styles.container}>
      <View style={styles.logoContainer}>
        <View style={styles.logoCircle}>
          <Text style={styles.logoText}>👑</Text>
        </View>
        <Text style={styles.title}>صيدلية النخبة</Text>
        <Text style={styles.subtitle}>الرعاية الملكية بين يديك</Text>
      </View>

      <View style={styles.formContainer}>
        <Text style={styles.inputLabel}>رقم الهاتف</Text>
        <TextInput
          style={styles.input}
          placeholder="07xxxxxxxx"
          placeholderTextColor="#a4b0be"
          value={phone}
          onChangeText={setPhone}
          keyboardType="phone-pad"
        />

        <Text style={styles.inputLabel}>كلمة المرور</Text>
        <TextInput
          style={styles.input}
          placeholder="••••••••"
          placeholderTextColor="#a4b0be"
          value={password}
          onChangeText={setPassword}
          secureTextEntry
        />

        <TouchableOpacity style={styles.button} onPress={handleLogin} disabled={loading}>
          {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>تسجيل الدخول الملكي</Text>}
        </TouchableOpacity>

        <TouchableOpacity onPress={() => navigation.navigate('Register')} style={styles.registerLink}>
          <Text style={styles.linkPrefix}>ليس لديك حساب؟</Text>
          <Text style={styles.linkText}>انضم للنخبة الآن</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 25,
    backgroundColor: '#0a3d62', // Deep Royal Navy
    justifyContent: 'center',
  },
  logoContainer: {
    alignItems: 'center',
    marginBottom: 50,
  },
  logoCircle: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: '#fff',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 20,
    elevation: 15,
  },
  logoText: {
    fontSize: 40,
  },
  title: {
    fontSize: 32,
    fontWeight: 'bold',
    color: '#fff',
    textAlign: 'center',
    letterSpacing: 1,
  },
  subtitle: {
    fontSize: 16,
    color: '#d1d8e0',
    textAlign: 'center',
    marginTop: 5,
  },
  formContainer: {
    backgroundColor: '#fff',
    padding: 25,
    borderRadius: 30,
    elevation: 20,
    shadowColor: '#000',
    shadowOpacity: 0.3,
    shadowRadius: 15,
  },
  inputLabel: {
    color: '#0a3d62',
    fontSize: 14,
    fontWeight: 'bold',
    marginBottom: 8,
    textAlign: 'right',
  },
  input: {
    backgroundColor: '#f1f2f6',
    padding: 15,
    borderRadius: 15,
    marginBottom: 20,
    textAlign: 'right',
    fontSize: 16,
    color: '#2f3542',
    borderWidth: 1,
    borderColor: '#ced6e0',
  },
  button: {
    backgroundColor: '#009432', // Emerald Green
    padding: 18,
    borderRadius: 15,
    alignItems: 'center',
    marginTop: 10,
    elevation: 5,
  },
  buttonText: {
    color: '#fff',
    fontSize: 18,
    fontWeight: 'bold',
  },
  registerLink: {
    flexDirection: 'row-reverse',
    justifyContent: 'center',
    marginTop: 25,
  },
  linkPrefix: {
    color: '#747d8c',
    fontSize: 14,
  },
  linkText: {
    color: '#0a3d62',
    fontWeight: 'bold',
    fontSize: 14,
    marginRight: 5,
  },
});

export default LoginScreen;
