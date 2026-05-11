import React, { useRef, useEffect } from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  withTiming,
} from 'react-native-reanimated';
import { Ionicons } from '@expo/vector-icons';

const tabs = [
  { key: 'Home', label: 'الرئيسية', icon: 'home' },
  { key: 'Cart', label: 'السلة', icon: 'cart' },
  { key: 'Orders', label: 'الطلبات', icon: 'receipt' },
];

interface Props {
  state: any;
  descriptors: any;
  navigation: any;
}

const TabIcon: React.FC<{ icon: string; active: boolean }> = ({ icon, active }) => {
  const scale = useSharedValue(active ? 1 : 0);
  const translateY = useSharedValue(active ? 0 : 0);

  useEffect(() => {
    if (active) {
      scale.value = withSpring(1.25, { damping: 8, stiffness: 150 });
      translateY.value = withSpring(-12, { damping: 10, stiffness: 180 });
    } else {
      scale.value = withTiming(1, { duration: 200 });
      translateY.value = withTiming(0, { duration: 200 });
    }
  }, [active]);

  const animStyle = useAnimatedStyle(() => ({
    transform: [
      { scale: scale.value },
      { translateY: translateY.value },
    ],
  }));

  return (
    <Animated.View style={[styles.iconWrap, active && styles.iconWrapActive, animStyle]}>
      <Ionicons
        name={(icon + (active ? '' : '-outline')) as any}
        size={22}
        color={active ? '#fff' : '#636e72'}
      />
    </Animated.View>
  );
};

const AnimatedTabBar: React.FC<Props> = ({ state, descriptors, navigation }) => {
  return (
    <View style={styles.container}>
      <View style={styles.inner}>
        {state.routes.map((route: any, index: number) => {
          const { options } = descriptors[route.key];
          const isFocused = state.index === index;
          const tab = tabs[index] || tabs[0];

          const onPress = () => {
            const event = navigation.emit({
              type: 'tabPress',
              target: route.key,
              canPreventDefault: true,
            });

            if (!isFocused && !event.defaultPrevented) {
              navigation.navigate(route.name);
            }
          };

          return (
            <TouchableOpacity
              key={route.key}
              activeOpacity={0.7}
              onPress={onPress}
              style={styles.tab}
            >
              <TabIcon icon={tab.icon} active={isFocused} />
              <Text style={[styles.label, isFocused && styles.labelActive]}>
                {tab.label}
              </Text>
            </TouchableOpacity>
          );
        })}
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: 'transparent',
    paddingHorizontal: 20,
    paddingBottom: 8,
    marginTop: -10,
  },
  inner: {
    flexDirection: 'row-reverse',
    backgroundColor: '#fff',
    borderRadius: 30,
    paddingVertical: 8,
    paddingHorizontal: 12,
    elevation: 12,
    shadowColor: '#000',
    shadowOpacity: 0.15,
    shadowRadius: 20,
    shadowOffset: { width: 0, height: -4 },
  },
  tab: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 4,
  },
  iconWrap: {
    width: 44,
    height: 44,
    borderRadius: 22,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'transparent',
  },
  iconWrapActive: {
    backgroundColor: '#0a3d62',
    elevation: 6,
    shadowColor: '#0a3d62',
    shadowOpacity: 0.4,
    shadowRadius: 8,
  },
  label: {
    fontSize: 10,
    fontWeight: '600',
    color: '#636e72',
    marginTop: 2,
  },
  labelActive: {
    color: '#0a3d62',
    fontWeight: '800',
  },
});

export default AnimatedTabBar;
